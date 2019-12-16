﻿#if NET40
using CodeArts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace CodeArts.Mvc
{
    /// <summary>
    /// 使用依赖注入（注入<see cref="ApiController"/>的构造函数参数类型，若引入了【CodeArts.ORM】，将会注入【<see cref="ApiController"/>的构造函数参数】以及【其参数类型的构造函数参数】中使用到的【数据仓库类型】）
    /// </summary>
    public class SkyDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SkyDependencyResolver()
        {
            var assemblys = AssemblyFinder.FindAll();

            var assemblyTypes = assemblys.SelectMany(x => x.GetTypes());

            var controllerTypes = assemblyTypes
                .Where(type => type.IsClass && !type.IsAbstract && typeof(ApiController).IsAssignableFrom(type));

            var interfaceTypes = controllerTypes
                .SelectMany(type =>
                {
                    return type.GetConstructors()
                          .SelectMany(x => x.GetParameters()
                          .Select(y => y.ParameterType));
                }).Distinct();

            var types = interfaceTypes.SelectMany(x => assemblyTypes.Where(y => x.IsAssignableFrom(y)));

            controllerTypes.ForEach(type =>
            {
                var typeStore = RuntimeTypeCache.Instance.GetCache(type);

                var storeItem = typeStore.ConstructorStores
                    .OrderBy(x => x.ParameterStores.Count)
                    .FirstOrDefault() ?? throw new BusiException($"接口({type.FullName})无公共构造函数!");

                var arguments = storeItem.ParameterStores.Select(x =>
                {
                    var argType = types.FirstOrDefault(y => y.IsClass && !y.IsAbstract && x.ParameterType.IsAssignableFrom(y)) ?? throw new BusiException($"未找到类型({x.ParameterType.FullName})依赖注入的实现类!");

                    try
                    {
                        return Expression.New(argType);
                    }
                    catch
                    {
                        throw new BusiException($"类型({argType.FullName})不包含公共无参构造函数!");
                    }
                });

                var newExp = arguments.Any() ? Expression.New(storeItem.Member, arguments) : Expression.New(storeItem.Member);

                var lamdaExp = Expression.Lambda<Func<object>>(newExp);

                Cache.Add(type, lamdaExp.Compile());
            });
        }

        private bool _disposed;

        private readonly static Dictionary<Type, Func<object>> Cache = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// 依赖注入范围
        /// </summary>
        /// <returns></returns>
        public IDependencyScope BeginScope() => this;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (Cache.TryGetValue(serviceType, out Func<object> invoke))
            {
                return invoke.Invoke();
            }

            return null;
        }
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            yield break;
        }
    }
}
#endif