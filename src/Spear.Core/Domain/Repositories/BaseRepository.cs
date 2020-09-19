using Spear.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Spear.Core.Domain.Repositories
{
    /// <summary> 基础仓储 </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract partial class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private const string KeyField = "Id";

        /// <summary> 构造函数 </summary>
        /// <param name="unitOfWork"></param>
        protected BaseRepository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
        /// <summary> 业务操作单元 </summary>
        public IUnitOfWork UnitOfWork { get; }

        /// <summary> 所有数据 </summary>
        public abstract IQueryable<TEntity> GetAll();

        /// <summary> 插入数据 </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract TKey Insert(TEntity entity);

        /// <summary> 批量插入数据 </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            return entities.Select(Insert).Count(key => key != null);
        }

        /// <summary> 删除数据 </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract int Delete(TEntity entity);

        /// <summary> 删除 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract int Delete(TKey key);

        /// <summary> 删除 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public abstract int Delete(Expression<Func<TEntity, bool>> expression);

        /// <summary> 更新 </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract int Update(TEntity entity);
        /// <summary> 更新 </summary>
        /// <param name="entity"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public abstract int Update(TEntity entity, params string[] parms);

        /// <summary> 更新 </summary>
        /// <param name="entity"></param>
        /// <param name="expression"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public int Update(TEntity entity, Expression<Func<TEntity, bool>> expression, params string[] parms)
        {
            return Update(entity, Where(expression), parms);
        }

        /// <summary> 更新 </summary>
        /// <param name="entity"></param>
        /// <param name="entities"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public abstract int Update(TEntity entity, IQueryable<TEntity> entities, params string[] parms);

        /// <summary> 更新 </summary>
        /// <param name="propExpression"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public abstract int Update(Expression<Func<TEntity, dynamic>> propExpression, params TEntity[] entities);

        /// <summary> 是否存在 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool Exists(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().Any(expression);
        }

        /// <summary> 加载数据 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual TEntity Load(TKey key)
        {
            var express = CreateEqualityExpressionForId(key);
            return GetAll().SingleOrDefault(express);
        }

        /// <summary> 第一条数据 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TEntity First(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().First(expression);
        }

        /// <summary> 第一条或默认 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().FirstOrDefault(expression);
        }

        /// <summary> 唯一一条 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TEntity Single(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().Single(expression);
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().SingleOrDefault(expression);
        }

        /// <summary> 主键批量加载 </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IQueryable<TEntity> List(IEnumerable<TKey> keys)
        {
            return GetAll().Where(t => keys.Contains(t.Id));
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        /// <summary> 分页 </summary>
        /// <param name="ordered"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public DResults<TEntity> PageList(IOrderedQueryable<TEntity> ordered, DPage page)
        {
            if (ordered == null)
                return DResult.Errors<TEntity>("数据查询异常！");
            var result = ordered.Skip(page.Page * page.Size).Take(page.Size).ToList();
            var total = ordered.Count();
            return DResult.Succ(result, total);
        }

        /// <summary> 统计数量 </summary>
        public int Count()
        {
            return GetAll().Count();
        }

        /// <summary> 统计数量 </summary>
        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().Count(predicate);
        }

        /// <summary> 统计数量 </summary>
        public long LongCount()
        {
            return GetAll().LongCount();
        }

        /// <summary> 统计数量 </summary>
        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().LongCount(predicate);
        }

        /// <summary> 最大值 </summary>
        public TValue Max<TValue>(Func<TEntity, TValue> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return default(TValue);
            return data.Max(perdicate);
        }

        /// <summary> 最小值 </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TValue Min<TValue>(Func<TEntity, TValue> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return default(TValue);
            return data.Min(perdicate);
        }

        /// <summary> 平均值 </summary>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public double Average(Func<TEntity, int> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return 0;
            return data.Average(perdicate);
        }

        /// <summary> 平均值 </summary>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public double Average(Func<TEntity, long> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return 0;
            return data.Average(perdicate);
        }

        /// <summary> 平均值 </summary>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public double Average(Func<TEntity, double> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return 0;
            return data.Average(perdicate);
        }

        /// <summary> 平均值 </summary>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public decimal Average(Func<TEntity, decimal> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return 0;
            return data.Average(perdicate);
        }

        /// <summary> 平均值 </summary>
        /// <param name="perdicate"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public float Average(Func<TEntity, float> perdicate, Expression<Func<TEntity, bool>> condition = null)
        {
            var data = condition == null ? GetAll() : GetAll().Where(condition);
            if (data == null || !data.Any())
                return 0;
            return data.Average(perdicate);
        }

        /// <summary> 创建主键lamda </summary>
        /// <param name="id"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, bool>> CreateEqualityExpressionForId(TKey id,
            string keyColumn = KeyField)
        {
            var lambdaParam = Expression.Parameter(typeof(TEntity));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, keyColumn),
                Expression.Constant(id, typeof(TKey))
                );

            return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
        }

    }
}
