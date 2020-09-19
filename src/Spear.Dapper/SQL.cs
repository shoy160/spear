using Dapper;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Data;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spear.Dapper
{
    /// <summary> sql语句辅助 目前只支持Sql Server</summary>
    public class SQL
    {
        private readonly StringBuilder _sqlBuilder;
        private readonly DynamicParameters _parameters;
        private readonly ILogger _logger;

        /// <summary> 实例化 </summary>
        /// <param name="sql"></param>
        public SQL(string sql)
        {
            _sqlBuilder = new StringBuilder(sql);
            _parameters = new DynamicParameters();
            _logger = CurrentIocManager.Resolve<ILoggerFactory>()?.CreateLogger<SQL>();
        }

        /// <summary> 追加sql </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public void Add(string sql)
        {
            _sqlBuilder.Append(sql.StartsWith(" ") ? sql : string.Concat(" ", sql));
        }

        /// <summary> 追加sql </summary>
        /// <param name="subSql"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SQL operator +(string subSql, SQL sql)
        {
            sql.Add(subSql);
            return sql;
        }

        /// <summary> 追加sql </summary>
        /// <param name="sql"></param>
        /// <param name="subSql"></param>
        /// <returns></returns>
        public static SQL operator +(SQL sql, string subSql)
        {
            sql.Add(subSql);
            return sql;
        }

        /// <summary> 实例化sql </summary>
        /// <param name="sql"></param>
        public static implicit operator SQL(string sql)
        {
            return new SQL(sql);
        }

        /// <summary> 添加参数 </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SQL this[string name, object value]
        {
            get
            {
                _parameters.Add(name, value);
                return this;
            }
        }

        /// <summary> 是否是更新操作 </summary>
        public bool IsChange()
        {
            return Regex.IsMatch(_sqlBuilder.ToString(), "((insert)|(update)|(delete))\\s+", RegexOptions.IgnoreCase);
        }

        /// <summary> 是否新增操作 </summary>
        public bool IsInsert()
        {
            return Regex.IsMatch(_sqlBuilder.ToString(), "insert\\s+", RegexOptions.IgnoreCase);
        }

        /// <summary> 是否是查询操作 </summary>
        public bool IsSelect()
        {
            return !IsChange() && Regex.IsMatch(_sqlBuilder.ToString(), "select\\s+", RegexOptions.IgnoreCase);
        }

        /// <summary> 获取查询列 </summary>
        private static string Columns(string sql)
        {
            var match = Regex.Match(sql,
                "select\\s(?<column>((?!select).)+(select((?!from).)+from((?!from).)+)*((?!from).)*)\\sfrom", RegexOptions.IgnoreCase);
            return match.Groups["column"].Value;
        }

        /// <summary> 查询最终的列名 </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        private static string NoAliasColums(string columns)
        {
            var columnList = new List<string>();
            //替换系统函数
            columns = Regex.Replace(columns, "((isnull)|(convert)|(cast)|(replace)|(exists))\\([^\\)]+\\)", string.Empty,
                RegexOptions.IgnoreCase);
            var list = columns.Split(',');
            foreach (var item in list)
            {
                var asArray = Regex.Split(item, "\\s+as\\s+", RegexOptions.IgnoreCase);
                if (asArray.Length > 1)
                    columnList.Add(asArray[1]);
                else
                {
                    asArray = item.Split('.');
                    columnList.Add(asArray.Length > 1 ? asArray[1] : item);
                }
            }
            return string.Join(",", columnList);
        }

        private string Where(string sql)
        {
            var match = Regex.Match(sql, "where\\s+(.+)order", RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }

        private static string Order(string sql)
        {
            var match = Regex.Match(sql, "order\\s+by\\s+(.+)$", RegexOptions.IgnoreCase);
            return match.Value;
        }

        /// <summary> 构造分页语句 </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="conn"></param>
        /// <param name="formatSql">格式化SQL</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public void Paged(int page, int size, IDbConnection conn, bool formatSql = true, string count = null)
        {
            if (!IsSelect())
                return;
            var sql = ToString();
            var columns = Columns(sql);
            var order = Order(sql);
            sql = conn.PagedSql(sql, columns, order, formatSql, count);
            if (page < 1) page = 1;

            _parameters.Add("skip", (page - 1) * size);
            _parameters.Add("size", size);
            _sqlBuilder.Clear();
            _sqlBuilder.Append(sql);
        }

        /// <summary> 自增主键 </summary>
        private void Identity(DbType type)
        {
            _sqlBuilder.Append(";SELECT @identity_id=SCOPE_IDENTITY();");
            _parameters.Add("@identity_id", dbType: type, direction: ParameterDirection.Output);
        }

        /// <summary> 插入自增主键数据 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T InsertIdentity<T>(IDbConnection conn, object param = null, IDbTransaction transaction = null)
            where T : struct
        {
            if (!IsInsert())
                return default(T);
            var type = typeof(T);
            DbType dbType;
            if (type == typeof(int))
                dbType = DbType.Int32;
            else if (type == typeof(long))
                dbType = DbType.Int64;
            else if (type == typeof(byte))
                dbType = DbType.Int16;
            else return default(T);
            Identity(dbType);
            if (param != null)
                _parameters.AddDynamicParams(param);
            var i = conn.Execute(ToString(), _parameters, transaction);
            return i > 0 ? _parameters.Get<T>("@identity_id") : default(T);
        }

        /// <summary> 分页列表 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="conn"></param>
        /// <param name="param"></param>
        /// <param name="formatSql"></param>
        /// <returns></returns>
        public PagedList<T> PagedList<T>(IDbConnection conn, int page, int size, object param = null, bool formatSql = true)
        {
            return PagedListAsync<T>(conn, page, size, param, formatSql).SyncRun();
        }

        /// <summary> 分页列表 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="conn"></param>
        /// <param name="param"></param>
        /// <param name="formatSql">格式化SQL</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<PagedList<T>> PagedListAsync<T>(IDbConnection conn, int page, int size, object param = null, bool formatSql = true, string count = null)
        {
            if (!IsSelect())
                return new PagedList<T>();
            Paged(page, size, conn, formatSql, count);
            if (param != null)
                _parameters.AddDynamicParams(param);
            var sql = ToString();
            using (var muli = await conn.QueryMultipleAsync(sql, _parameters))
            {
                var list = await muli.ReadAsync<T>() ?? new List<T>();
                var total = await muli.ReadFirstOrDefaultAsync<long?>();
                return new PagedList<T>(list.ToArray(), page, size, (int)(total ?? 0));
            }
        }

        /// <summary> 获取动态参数 </summary>
        /// <returns></returns>
        public DynamicParameters Parameters()
        {
            return _parameters;
        }

        public string FormatSql(IDbConnection conn)
        {
            return conn.FormatSql(ToString());
        }

        /// <summary> 返回sql语句 </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sql = _sqlBuilder.ToString();
            sql = sql.Replace("\r\n", " ", RegexOptions.IgnoreCase);
            sql = sql.Replace("[\\s]+", " ", RegexOptions.IgnoreCase);
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(sql);
            return sql;
        }
    }
}
