using Dapper;
using Spear.Core;
using Spear.Core.Data;
using Spear.Core.Dependency;
using Spear.Core.Domain;
using Spear.Core.Domain.Entities;
using Spear.Core.Extensions;
using Spear.Core.Serialize;
using Spear.Dapper.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Spear.Dapper
{
    /// <summary> 主键属性 </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary> Dapper自定义扩展 </summary>
    public static partial class DapperExtension
    {
        #region 私有属性

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypePropsCache =
            new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IDictionary<string, string>> FieldsCache =
            new ConcurrentDictionary<RuntimeTypeHandle, IDictionary<string, string>>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertCache =
            new ConcurrentDictionary<RuntimeTypeHandle, string>();

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, KeyValuePair<string, string>> KeyCache =
            new ConcurrentDictionary<RuntimeTypeHandle, KeyValuePair<string, string>>();

        #endregion

        #region 私有方法

        private static List<PropertyInfo> Props(Type modelType)
        {
            if (TypePropsCache.TryGetValue(modelType.TypeHandle, out var props))
                return props.ToList();
            props = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            TypePropsCache.TryAdd(modelType.TypeHandle, props);
            return props.ToList();
        }

        private static KeyValuePair<string, string> GetKey(Type modelType)
        {
            const string id = "id";
            if (KeyCache.TryGetValue(modelType.TypeHandle, out var key))
                return key;
            var props = Props(modelType);
            var attr = modelType.GetCustomAttribute<NamingAttribute>();
            var naming = attr?.NamingType;
            var keyProp = props.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            if (keyProp == null)
            {
                keyProp = props.FirstOrDefault(p =>
                    string.Equals(p.PropName(), id, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(p.Name, id, StringComparison.CurrentCultureIgnoreCase));
            }

            key = new KeyValuePair<string, string>(keyProp?.Name ?? id, keyProp?.PropName(naming) ?? id);
            KeyCache.TryAdd(modelType.TypeHandle, key);
            return key;
        }

        private static Type GetInnerType<T>()
        {
            var type = typeof(T);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }

        #endregion

        #region SQL
        ///// <summary> 查询到DataSet </summary>
        ///// <param name="conn"></param>
        ///// <param name="sql"></param>
        ///// <param name="param"></param>
        ///// <param name="commandTimeout"></param>
        ///// <param name="commandType"></param>
        ///// <returns></returns>
        //public static DataSet QueryDataSet(this IDbConnection conn, string sql, object param = null,
        //    int? commandTimeout = null,
        //    CommandType? commandType = null)
        //{
        //    var reader = conn.ExecuteReader(sql, param, null, commandTimeout, commandType);
        //    var dataset = new XDataSet();
        //    dataset.Load(reader, LoadOption.OverwriteChanges, null, new DataTable[] { });
        //    return dataset;
        //}

        /// <summary> 字段列表 </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static IDictionary<string, string> TypeProperties(this Type modelType)
        {
            if (FieldsCache.TryGetValue(modelType.TypeHandle, out var dict))
                return dict;
            var attr = modelType.GetCustomAttribute<NamingAttribute>();
            var naming = attr?.NamingType;
            var props = Props(modelType);
            var fields = props.ToDictionary(k => k.Name, v => v.PropName(naming));
            FieldsCache.TryAdd(modelType.TypeHandle, fields);
            return fields;
        }

        /// <summary> 获取所有列名(as) </summary>
        /// <param name="modelType"></param>
        /// <param name="excepts">排除的字段</param>
        /// <param name="includes">包含的字段</param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public static string Columns(this Type modelType, string[] excepts = null, string[] includes = null,
            string tableAlias = null)
        {
            var props = modelType.TypeProperties();
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                if (excepts != null && excepts.Contains(prop.Key))
                    continue;
                if (includes != null && !includes.Contains(prop.Key))
                    continue;
                if (prop.Key.Equals(prop.Value, StringComparison.CurrentCultureIgnoreCase))
                    sb.AppendFormat(
                        string.IsNullOrWhiteSpace(tableAlias) ? "[{0}]," : string.Concat(tableAlias, ".[{0}],"),
                        prop.Value);
                else
                    sb.AppendFormat(
                        string.IsNullOrWhiteSpace(tableAlias)
                            ? "[{0}] AS [{1}],"
                            : string.Concat(tableAlias, ".[{0}] AS [{1}],"), prop.Value, prop.Key);
            }

            return sb.ToString().TrimEnd(',');
        }

        /// <summary> 生成insert语句 </summary>
        /// <returns></returns>
        public static string InsertSql(this Type modelType, string[] excepts = null)
        {
            if (InsertCache.TryGetValue(modelType.TypeHandle, out var sql))
                return sql;

            var tableName = modelType.PropName();
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO [{tableName}]");

            var fields = TypeProperties(modelType);
            if (excepts != null && excepts.Any())
                fields = fields.Where(t => !excepts.Contains(t.Key)).ToDictionary(k => k.Key, v => v.Value);
            var fieldSql = string.Join(",", fields.Select(t => $"[{t.Value}]"));
            var paramSql = string.Join(",", fields.Select(t => $"@{t.Key}"));
            sb.Append($" ({fieldSql}) VALUES ({paramSql})");
            sql = sb.ToString();
            InsertCache.TryAdd(modelType.TypeHandle, sql);
            return sql;
        }

        /// <summary> 设置对象属性 </summary>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetPropValue(this object model, string propName, object value)
        {
            var type = model.GetType();
            var prop = type.GetProperty(propName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
                prop.SetValue(model, value);
        }

        /// <summary> 获取对象属性 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T PropValue<T>(this object model, string propName)
        {
            var type = model.GetType();
            var prop = type.GetProperty(propName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop == null ? default(T) : prop.GetValue(model).CastTo<T>();
        }

        /// <summary> 获取对象属性 </summary>
        /// <param name="model"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string PropValue(this object model, string propName)
        {
            return PropValue<string>(model, propName);
        }
        #endregion

        #region Query

        private static string QueryAllSql<T>()
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var columns = type.Columns();
            var sql = $"SELECT {columns} FROM [{tableName}]";
            return sql;
        }

        private static string QueryByIdSql<T>(string keyColumn = null)
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var columns = type.Columns();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"SELECT {columns} FROM [{tableName}] WHERE [{keyName}]=@id";
            return sql;
        }

        /// <summary> 查询所有数据 </summary>
        public static IEnumerable<T> QueryAll<T>(this IDbConnection conn)
            where T : IEntity
        {
            var sql = QueryAllSql<T>();
            sql = conn.FormatSql(sql);
            return conn.Query<T>(sql);
        }

        /// <summary> 根据主键查询单条 </summary>
        /// <param name="conn"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <returns></returns>
        public static T QueryById<T>(this IDbConnection conn, object key, string keyColumn = null)
            where T : IEntity
        {
            var sql = QueryByIdSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return conn.QueryFirstOrDefault<T>(sql, new { id = key });
        }

        /// <summary> 分页 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static PagedList<T> PagedList<T>(this IDbConnection conn, string sql, int page, int size,
            object param = null)
        {
            SQL pageSql = sql;
            return pageSql.PagedList<T>(conn, page, size, param);
        }

        #endregion

        #region Insert

        /// <summary> 插入单条数据,不支持有自增列 </summary>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="excepts">过滤项(如：自增ID)</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection conn, T model, string[] excepts = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, model, trans, commandTimeout);
        }

        /// <summary> 批量插入 </summary>
        /// <param name="conn"></param>
        /// <param name="models"></param>
        /// <param name="excepts"></param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection conn, IEnumerable<T> models, string[] excepts = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var type = typeof(T);
            var sql = type.InsertSql(excepts);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, models.ToArray(), trans, commandTimeout);
        }

        public static int BatchInsert<T>(this IDbConnection conn, IEnumerable<T> models, string[] excepts = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            return conn.Insert(models, excepts, trans, commandTimeout);
        }

        #endregion

        #region Update

        private static string UpdateSql<T>(string[] updateProps = null)
        {
            var type = GetInnerType<T>();
            var tableName = type.PropName();
            var props = type.TypeProperties();
            var key = GetKey(type);
            var sb = new StringBuilder();
            sb.Append($"UPDATE [{tableName}] SET ");
            foreach (var prop in props)
            {
                if (prop.Key == key.Key || updateProps != null && !updateProps.Contains(prop.Key) &&
                    !updateProps.Contains(prop.Value))
                    continue;
                sb.Append($"[{prop.Value}]=@{prop.Key},");
            }

            sb.Remove(sb.Length - 1, 1);

            sb.Append($" WHERE [{key.Value}]=@{key.Key}");
            return sb.ToString();
        }

        /// <summary> 批量更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdates">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection conn, IEnumerable<T> entityToUpdates, string[] updateProps = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, entityToUpdates.ToArray(), trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdates">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection conn, IEnumerable<T> entityToUpdates, Expression<Func<T, object>> fieldsExpression,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var updateProps = fieldsExpression.MemberNames()?.ToArray();
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, entityToUpdates.ToArray(), trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection conn, T entityToUpdate, string[] updateProps = null,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, entityToUpdate, trans, commandTimeout);
        }

        /// <summary> 更新数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="entityToUpdate">待更新实体</param>
        /// <param name="updateProps">更新属性</param>
        /// <param name="trans"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection conn, T entityToUpdate, Expression<Func<T, object>> fieldsExpression,
            IDbTransaction trans = null, int? commandTimeout = null)
            where T : IEntity
        {
            var updateProps = fieldsExpression.MemberNames()?.ToArray();
            var sql = UpdateSql<T>(updateProps);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, entityToUpdate, trans, commandTimeout);
        }

        #endregion

        #region Delete

        private static string DeleteSql<T>(string keyColumn = null)
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"DELETE FROM [{tableName}] WHERE [{keyName}]=@value";
            return sql;
        }

        /// <summary> 删除数据 </summary>
        /// <param name="conn">连接</param>
        /// <param name="value">列值</param>
        /// <param name="keyColumn">列名</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection conn, object value, string keyColumn = null,
            IDbTransaction trans = null)
            where T : IEntity
        {
            var sql = DeleteSql<T>(keyColumn);
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, new { value }, trans);
        }

        /// <summary> 删除 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int DeleteWhere<T>(this IDbConnection conn, string where, object param = null,
            IDbTransaction trans = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            var sql = $"DELETE FROM [{tableName}] WHERE {where}";
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, param, trans);
        }

        #endregion

        #region Common

        /// <summary> 统计总数 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Count<T>(this IDbConnection conn, string column, object value)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            var sql = $"SELECT COUNT(1) FROM [{tableName}] WHERE [{column}]=@value";
            sql = conn.FormatSql(sql);
            return conn.QueryFirstOrDefault<long>(sql, new { value });
        }

        /// <summary> 统计总数 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long CountWhere<T>(this IDbConnection conn, string where = null, object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT COUNT(1) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr, param);
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists<T>(this IDbConnection conn, string column, object value)
            where T : IEntity
        {
            return conn.Count<T>(column, value) > 0;
        }

        /// <summary> 是否存在 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool ExistsWhere<T>(this IDbConnection conn, string where = null, object param = null)
            where T : IEntity
        {
            return conn.CountWhere<T>(where, param) > 0;
        }

        /// <summary> 最小 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Min<T>(this IDbConnection conn, string column, string where = null, object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MIN([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr, param);
        }

        /// <summary> 最大 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Max<T>(this IDbConnection conn, string column, string where = null, object param = null)
            where T : IEntity
        {
            var tableName = typeof(T).PropName();
            SQL sql = $"SELECT MAX([{column}]) FROM [{tableName}]";
            if (!string.IsNullOrWhiteSpace(where))
                sql += $"WHERE {where}";
            var sqlStr = conn.FormatSql(sql.ToString());
            return conn.QueryFirstOrDefault<long>(sqlStr, param);
        }

        /// <summary> 自增数据 </summary>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <param name="count"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int Increment<T>(this IDbConnection conn, string column, object key, string keyColumn = null,
            int count = 1, IDbTransaction trans = null)
            where T : IEntity
        {
            var type = typeof(T);
            var tableName = type.PropName();
            var keyName = string.IsNullOrWhiteSpace(keyColumn) ? GetKey(type).Value : keyColumn;
            var sql = $"UPDATE [{tableName}] SET [{column}]=[{column}] + @count WHERE [{keyName}]=@id";
            sql = conn.FormatSql(sql);
            return conn.Execute(sql, new { id = key, count }, trans);
        }

        /// <summary> 转换DataTable </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据列表</param>
        /// <param name="formatHeader">表头信息</param>
        /// <param name="tableName">表名</param>
        /// <param name="excepts">排除字段</param>
        /// <param name="exceptNoHeader">排除没有声明header的属性，默认True</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> data, Func<string, string> formatHeader = null,
            string tableName = null, string[] excepts = null, bool exceptNoHeader = true)
        {
            var type = GetInnerType<T>();
            tableName = string.IsNullOrWhiteSpace(tableName) ? type.PropName() : tableName;
            var dt = new DataTable(tableName);
            var props = Props(type);
            if (excepts != null && excepts.Any())
                props = props.Where(t => !excepts.Contains(t.Name)).ToList();
            var headers = new Dictionary<PropertyInfo, string>();
            using (var getters = new GetterHelper<T>())
            {
                foreach (var prop in props)
                {
                    string key;
                    if (formatHeader != null)
                        key = formatHeader(prop.Name);
                    else
                    {
                        var desc = prop.GetCustomAttribute<DescriptionAttribute>();
                        if (desc == null)
                        {
                            if (exceptNoHeader)
                                continue;
                            key = prop.Name;
                        }
                        else
                        {
                            key = desc.Description;
                        }
                    }

                    headers.Add(prop, key);
                    getters.AddGetter(prop);
                    dt.Columns.Add(key, prop.PropertyType.GetUnNullableType());
                    dt.Columns[key].AllowDBNull = true;
                }

                foreach (var item in data)
                {
                    var row = dt.NewRow();
                    foreach (var header in headers)
                    {
                        var value = getters.GetValue(item, header.Key) ?? DBNull.Value;
                        row[header.Value] = value;
                    }

                    dt.Rows.Add(row);
                }
            }

            return dt;
        }

        #endregion

        /// <summary> 获取仓储对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public static T Repository<T>(this DService service) where T : DRepository
        {
            return CurrentIocManager.Resolve<T>();
        }
    }
}
