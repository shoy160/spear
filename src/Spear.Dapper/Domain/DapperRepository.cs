using Spear.Core.Dependency;
using Spear.Core.Domain;
using Spear.Core.Domain.Entities;
using Spear.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Spear.Dapper.Domain
{
    /// <summary> 基础仓储 </summary>
    /// <typeparam name="T"></typeparam>
    public partial class DapperRepository<T> : DRepository
        where T : IEntity
    {
        /// <summary> 仓储数据库实体类型 </summary>
        protected Type ModelType { get; }

        /// <summary> 构造函数 </summary>
        public DapperRepository() : this(CurrentIocManager.Resolve<IUnitOfWork>()) { }

        /// <summary> 构造 </summary>
        /// <param name="unitOfWork"></param>
        public DapperRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            ModelType = typeof(T);
        }

        /// <summary> 当前SELECT语句构建 </summary>
        /// <param name="where">where</param>
        /// <param name="orderby">order by</param>
        /// <param name="excepts">排除字段</param>
        /// <param name="includes">包含字段</param>
        /// <param name="tableAlias">表别名</param>
        /// <returns></returns>
        public string Select(string where, string orderby = null, string[] excepts = null, string[] includes = null,
            string tableAlias = null)
        {
            return ModelType.Select(where, orderby, excepts, includes, tableAlias);
        }

        /// <summary> 查询所有数据 </summary>
        public IEnumerable<T> Query()
        {
            return Connection.QueryAll<T>();
        }

        /// <summary> 根据主键查询单条 </summary>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public T QueryById(object key, string keyColumn = null)
        {
            return Connection.QueryById<T>(key, keyColumn);
        }

        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <returns></returns>
        public int Insert(T model, string[] excepts = null)
        {
            return TransConnection.Insert(model, excepts, Trans);
        }

        /// <summary> 批量插入 </summary>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <returns></returns>
        public int Insert(IEnumerable<T> models, string[] excepts = null)
        {
            return TransConnection.Insert<T>(models, excepts, Trans);
        }

        /// <summary> 更新 </summary>
        /// <param name="model"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public int Update(T model, string[] props = null)
        {
            return TransConnection.Update(model, props, Trans);
        }

        /// <summary> 更新 </summary>
        /// <param name="model"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public int Update(T model, Expression<Func<T, object>> propExpression)
        {
            var props = propExpression.MemberNames()?.ToArray();
            return TransConnection.Update(model, props, Trans);
        }

        /// <summary> 批量更新 </summary>
        /// <param name="model"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public int Update(IEnumerable<T> models, string[] props = null)
        {
            return TransConnection.Update(models, props, Trans);
        }

        /// <summary> 批量更新 </summary>
        /// <param name="model"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public int Update(IEnumerable<T> models, Expression<Func<T, object>> propExpression)
        {
            var props = propExpression.MemberNames()?.ToArray();
            return TransConnection.Update(models, props, Trans);
        }

        /// <summary> 删除 </summary>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public int Delete(object key, string keyColumn = null)
        {
            return TransConnection.Delete<T>(key, keyColumn, Trans);
        }
    }
}
