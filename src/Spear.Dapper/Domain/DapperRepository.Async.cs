using System;
using Spear.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Spear.Core.Extensions;

namespace Spear.Dapper.Domain
{
    public partial class DapperRepository<T> where T : IEntity
    {
        /// <summary> 查询所有数据 </summary>
        public Task<IEnumerable<T>> QueryAsync()
        {
            return Connection.QueryAllAsync<T>();
        }

        /// <summary> 根据主键查询单条 </summary>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public Task<T> QueryByIdAsync(object key, string keyColumn = null)
        {
            return Connection.QueryByIdAsync<T>(key, keyColumn);
        }

        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <returns></returns>
        public Task<int> InsertAsync(T model, string[] excepts = null)
        {
            return TransConnection.InsertAsync(model, excepts, Trans);
        }

        /// <summary> 批量插入 </summary>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <returns></returns>
        public Task<int> InsertAsync(IEnumerable<T> models, string[] excepts = null)
        {
            return TransConnection.InsertAsync(models, excepts, Trans);
        }

        /// <summary> 更新数据 </summary>
        /// <param name="model"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Task<int> UpdateAsync(T model, string[] props = null)
        {
            return TransConnection.UpdateAsync(model, props, Trans);
        }

        /// <summary> 更新数据 </summary>
        /// <param name="model"></param>
        /// <param name="propExpression"></param>
        /// <returns></returns>
        public Task<int> UpdateAsync(T model, Expression<Func<T, object>> propExpression)
        {
            var props = propExpression.MemberNames()?.ToArray();
            return TransConnection.UpdateAsync(model, props, Trans);
        }

        /// <summary> 批量更新数据 </summary>
        /// <param name="models"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public Task<int> UpdateAsync(IEnumerable<T> models, string[] props = null)
        {
            return TransConnection.UpdateAsync(models, props, Trans);
        }

        /// <summary> 批量更新数据 </summary>
        /// <param name="models"></param>
        /// <param name="propExpression"></param>
        /// <returns></returns>
        public Task<int> UpdateAsync(IEnumerable<T> models, Expression<Func<T, object>> propExpression)
        {
            var props = propExpression.MemberNames()?.ToArray();
            return TransConnection.UpdateAsync(models, props, Trans);
        }

        /// <summary> 删除 </summary>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public Task<int> DeleteAsync(object key, string keyColumn = null)
        {
            return TransConnection.DeleteAsync<T>(key, keyColumn, Trans);
        }
    }
}
