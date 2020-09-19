using Dapper;
using Spear.Core;
using Spear.Core.Data;
using Spear.Core.Domain.Entities;
using Spear.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Spear.Dapper
{
    public static partial class DapperExtension
    {
        /// <summary> 查询所有数据 </summary>
        public static async Task<IEnumerable<T>> QueryAllAsync<T>(this IDbConnection conn)
            where T : IEntity
        {
            var sql = QueryAllSql<T>();
            sql = conn.FormatSql(sql);
            return await conn.QueryAsync<T>(sql);
        }

        /// <summary> 根据主键查询单条 </summary>
        /// <param name="conn"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public static async Task<T> QueryByIdAsync<T>(this IDbConnection conn, object key, string keyColumn = null)
            where T : IEntity
        {
            var sql = QueryByIdSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return await conn.QueryFirstOrDefaultAsync<T>(sql, new { id = key });
        }

        /// <summary> 分页异步 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="param"></param>
        /// <param name="formatSql"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static async Task<PagedList<T>> PagedListAsync<T>(this IDbConnection conn, string sql, int page,
            int size, object param = null, bool formatSql = true, string count = null)
        {
            SQL pageSql = sql;
            return await pageSql.PagedListAsync<T>(conn, page, size, param, formatSql, count);
        }

        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> InsertAsync<T>(this IDbConnection conn, T model, string[] excepts = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, model, trans, commandTimeout);
        }

        /// <summary> 批量插入 </summary>
        /// <param name="conn"></param>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> InsertAsync<T>(this IDbConnection conn, IEnumerable<T> models,
            string[] excepts = null, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, models.ToArray(), trans, commandTimeout);
        }

        /// <summary> 批量新增 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> BatchInsertAsync<T>(this IDbConnection conn, IEnumerable<T> models,
            string[] excepts = null, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            return await conn.InsertAsync(models, excepts, trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, T entityToUpdate,
            string[] updateProps = null, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, entityToUpdate, trans, commandTimeout);
        }

        /// <summary> 批量更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, IEnumerable<T> entityToUpdate,
            string[] updateProps = null, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, entityToUpdate.ToArray(), trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="fieldsExpression">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, T entityToUpdate,
            Expression<Func<T, object>> fieldsExpression, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var updateProps = fieldsExpression.MemberNames()?.ToArray();
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, entityToUpdate, trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="fieldsExpression">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, IEnumerable<T> entityToUpdate,
            Expression<Func<T, object>> fieldsExpression, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var updateProps = fieldsExpression.MemberNames()?.ToArray();
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, entityToUpdate.ToArray(), trans, commandTimeout);
        }

        /// <summary> 删除数据 </summary>
        /// <param name="conn">连接</param>
        /// <param name="value">列值</param>
        /// <param name="keyColumn">列名</param>
        /// <param name="trans">事务</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this IDbConnection conn, object value, string keyColumn = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var sql = DeleteSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, new { value }, trans, commandTimeout);
        }

        /// <summary> 删除 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> DeleteWhereAsync<T>(this IDbConnection conn, string where, object param = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            var sql = $"DELETE FROM [{tableName}] WHERE {where}";
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, param, trans, commandTimeout);
        }

        /// <summary> 统计数量 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<long> CountAsync<T>(this IDbConnection conn, string column, object value)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            var sql = $"SELECT COUNT(1) FROM [{tableName}] WHERE [{column}]=@value";
            sql = conn.FormatSql(sql);
            return await conn.QueryFirstOrDefaultAsync<long>(sql, new { value });
        }

        /// <summary> 统计数量 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static async Task<long> CountWhereAsync<T>(this IDbConnection conn, string where = null,
            object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT COUNT(1) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return await conn.QueryFirstOrDefaultAsync<long>(sqlStr, param);
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> ExistsAsync<T>(this IDbConnection conn, string column, object value)
            where T : IEntity
        {
            return await conn.CountAsync<T>(column, value) > 0;
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static async Task<bool> ExistsWhereAsync<T>(this IDbConnection conn, string where = null,
            object param = null)
            where T : IEntity
        {
            return await conn.CountWhereAsync<T>(where, param) > 0;
        }

        /// <summary> 最小 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static async Task<long> MinAsync<T>(this IDbConnection conn, string column, string where = null,
            object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MIN([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return await conn.QueryFirstOrDefaultAsync<long>(sqlStr, param);
        }

        /// <summary> 最大 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static async Task<long> MaxAsync<T>(this IDbConnection conn, string column, string where = null,
            object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MAX([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return await conn.QueryFirstOrDefaultAsync<long>(sqlStr);
        }

        /// <summary> 自增数据 </summary>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <param name="count"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> IncrementAsync<T>(this IDbConnection conn, string column, object key,
            string keyColumn = null, int count = 1, IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"UPDATE [{tableName}] SET [{column}]=[{column}] + @count WHERE [{keyName}]=@id";
            sql = conn.FormatSql(sql);
            return await conn.ExecuteAsync(sql, new { id = key, count }, trans, commandTimeout);
        }
    }
}
