﻿using CodeArts.Emit.Expressions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CodeArts.Emit
{
    /// <summary>
    /// 构造函数。
    /// </summary>
    public class ConstructorEmitter : BlockAst
    {
        private int parameterIndex = 0;
        private readonly List<ParamterEmitter> parameters = new List<ParamterEmitter>();
        private readonly AbstractTypeEmitter typeEmitter;

        private class ConstructorExpression : AstExpression
        {
            private readonly ConstructorInfo constructor;
            private readonly ParamterEmitter[] parameters;

            public ConstructorExpression(ConstructorInfo constructor) : base(constructor.DeclaringType)
            {
                this.constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            }
            public ConstructorExpression(ConstructorInfo constructor, ParamterEmitter[] parameters) : this(constructor)
            {
                this.parameters = parameters;
            }

            public override void Load(ILGenerator ilg)
            {
                ilg.Emit(OpCodes.Ldarg_0);

                if (parameters != null)
                {
                    foreach (var exp in parameters)
                    {
                        exp.Load(ilg);
                    }
                }

                ilg.Emit(OpCodes.Call, constructor);
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="typeEmitter">父类型。</param>
        /// <param name="attributes">属性。</param>
        public ConstructorEmitter(AbstractTypeEmitter typeEmitter, MethodAttributes attributes) : this(typeEmitter, attributes, CallingConventions.Standard)
        {
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="typeEmitter">父类型。</param>
        /// <param name="attributes">属性。</param>
        /// <param name="conventions">调用约定。</param>
        public ConstructorEmitter(AbstractTypeEmitter typeEmitter, MethodAttributes attributes, CallingConventions conventions) : base(typeof(void))
        {
            this.typeEmitter = typeEmitter;
            Attributes = attributes;
            Conventions = conventions;
        }

        /// <summary>
        /// 方法的名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 方法的属性。
        /// </summary>
        public MethodAttributes Attributes { get; }

        /// <summary>
        /// 调用约定。
        /// </summary>
        public CallingConventions Conventions { get; }

        /// <summary>
        /// 参数。
        /// </summary>
        public ParamterEmitter[] Parameters => parameters.ToArray();

        /// <summary>
        /// 声明参数。
        /// </summary>
        /// <param name="parameterInfo">参数。</param>
        /// <returns></returns>
        public ParamterEmitter DefineParameter(ParameterInfo parameterInfo)
        {
            var parameter = DefineParameter(parameterInfo.ParameterType, parameterInfo.Attributes, parameterInfo.Name);

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
            if (parameterInfo.HasDefaultValue)
#else
            if (parameterInfo.IsOptional)
#endif
            {
                parameter.SetConstant(parameterInfo.DefaultValue);
            }

            foreach (var customAttribute in parameterInfo.GetCustomAttributesData())
            {
                parameter.SetCustomAttribute(customAttribute);
            }

            return parameter;
        }

        /// <summary>
        /// 声明参数。
        /// </summary>
        /// <param name="parameterType">参数类型。</param>
        /// <param name="attributes">属性。</param>
        /// <param name="strParamName">名称。</param>
        /// <returns></returns>
        public ParamterEmitter DefineParameter(Type parameterType, ParameterAttributes attributes, string strParamName)
        {
            var parameter = new ParamterEmitter(parameterType, ++parameterIndex, attributes, strParamName);
            parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 调用父类构造函数。
        /// </summary>
        public void InvokeBaseConstructor()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var type = typeEmitter.BaseType;

            if (type.IsGenericParameter)
            {
                type = type.GetGenericTypeDefinition();
            }

            InvokeBaseConstructor(type.GetConstructor(flags, null, Type.EmptyTypes, null));
        }

        /// <summary>
        /// 调用父类构造函数。
        /// </summary>
        /// <param name="constructor">构造函数。</param>
        public void InvokeBaseConstructor(ConstructorInfo constructor) => Append(new ConstructorExpression(constructor));

        /// <summary>
        /// 调用父类构造函数。
        /// </summary>
        /// <param name="constructor">构造函数。</param>
        /// <param name="parameters">参数。</param>
        public void InvokeBaseConstructor(ConstructorInfo constructor, params ParamterEmitter[] parameters) => Append(new ConstructorExpression(constructor, parameters));

        /// <summary>
        /// 发行。
        /// </summary>
        public void Emit(ConstructorBuilder builder)
        {
#if NET40
            var attributes = builder.GetMethodImplementationFlags();
#else
            var attributes = builder.MethodImplementationFlags;
#endif

            if ((attributes & MethodImplAttributes.Runtime) != MethodImplAttributes.IL)
            {
                return;
            }

            if (IsEmpty)
            {
                InvokeBaseConstructor();
            }

            foreach (var item in parameters)
            {
                item.Emit(builder.DefineParameter(item.Position, item.Attributes, item.ParameterName));
            }

            base.Load(builder.GetILGenerator());
        }
    }
}
