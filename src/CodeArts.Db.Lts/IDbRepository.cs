﻿using CodeArts.Db.Lts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if NET_NORMAL || NET_CORE
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CodeArts.Db
{
    /// <summary>
    /// 读写仓储。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IDRepository<TEntity> : IRepository<TEntity>, IOrderedQueryable<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IOrderedQueryable, IQueryable, IEnumerable where TEntity : class, IEntiy
    {
        /// <summary>
        /// 数据来源。
        /// </summary>
        /// <param name="table">表。</param>
        /// <returns></returns>
        IDRepository<TEntity> From(Func<ITableInfo, string> table);

        /// <summary>
        /// 条件。
        /// </summary>
        /// <param name="expression">表达式。</param>
        /// <returns></returns>
        IDRepository<TEntity> Where(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 获取或设置在终止尝试执行命令并生成错误之前的等待时间。
        /// </summary>
        /// <param name="commandTimeout">超时时间。</param>
        /// <returns></returns>
        IDRepository<TEntity> TimeOut(int commandTimeout);

        /// <summary>
        /// 更新数据。
        /// </summary>
        /// <param name="updateExp">更新的字段和值。</param>
        /// <returns></returns>
        int Update(Expression<Func<TEntity, TEntity>> updateExp);

        /// <summary>
        /// 删除数据。
        /// </summary>
        /// <returns></returns>
        int Delete();

        /// <summary>
        /// 删除数据。
        /// </summary>
        /// <param name="whereExp">条件表达式。</param>
        /// <returns></returns>
        int Delete(Expression<Func<TEntity, bool>> whereExp);

        /// <summary>
        /// 插入数据。
        /// </summary>
        /// <param name="querable">需要插入的数据。</param>
        /// <returns></returns>
        int Insert(IQueryable<TEntity> querable);

#if NET_NORMAL || NET_CORE
        /// <summary>
        /// 更新数据。
        /// </summary>
        /// <param name="updateExp">更新的字段和值。</param>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> updateExp, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除数据。
        /// </summary>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除数据。
        /// </summary>
        /// <param name="whereExp">条件表达式。</param>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExp, CancellationToken cancellationToken = default);

        /// <summary>
        /// 插入数据。
        /// </summary>
        /// <param name="querable">需要插入的数据。</param>
        /// <param name="cancellationToken">取消。</param>
        /// <returns></returns>
        Task<int> InsertAsync(IQueryable<TEntity> querable, CancellationToken cancellationToken = default);
#endif
    }

    /// <summary>
    /// 读写仓库（支持执行器）。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    public interface IDbRepository<TEntity> : IDRepository<TEntity>, IRepository<TEntity>, IOrderedQueryable<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IOrderedQueryable, IQueryable, IEnumerable where TEntity : class, IEntiy
    {
        /// <summary>
        /// 插入路由执行器。
        /// </summary>
        /// <param name="entry">项目。</param>
        /// <returns></returns>
        IInsertable<TEntity> AsInsertable(TEntity entry);

        /// <summary>
        /// 插入路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IInsertable<TEntity> AsInsertable(List<TEntity> entries);

        /// <summary>
        /// 插入路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IInsertable<TEntity> AsInsertable(TEntity[] entries);

        /// <summary>
        /// 更新路由执行器。
        /// </summary>
        /// <param name="entry">项目。</param>
        /// <returns></returns>
        IUpdateable<TEntity> AsUpdateable(TEntity entry);

        /// <summary>
        /// 更新路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IUpdateable<TEntity> AsUpdateable(List<TEntity> entries);

        /// <summary>
        /// 更新路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IUpdateable<TEntity> AsUpdateable(TEntity[] entries);

        /// <summary>
        /// 删除路由执行器。
        /// </summary>
        /// <param name="entry">项目。</param>
        /// <returns></returns>
        IDeleteable<TEntity> AsDeleteable(TEntity entry);

        /// <summary>
        /// 删除路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IDeleteable<TEntity> AsDeleteable(List<TEntity> entries);

        /// <summary>
        /// 删除路由执行器。
        /// </summary>
        /// <param name="entries">项目集合。</param>
        /// <returns></returns>
        IDeleteable<TEntity> AsDeleteable(TEntity[] entries);

    }
}