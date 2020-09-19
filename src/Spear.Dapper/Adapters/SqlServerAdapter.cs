using Spear.Core.Data;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Spear.Dapper.Adapters
{
    public class SqlServerAdapter : IDbConnectionAdapter
    {
        public const string Name = "SqlServer";

        /// <summary> 适配器名称 </summary>
        public string ProviderName => Name;

        /// <summary> 连接类型 </summary>
        public Type ConnectionType => typeof(SqlConnection);

        /// <summary> 格式化SQL </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string FormatSql(string sql)
        {
            return sql;
        }

        /// <summary> 构造分页SQL </summary>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string PageSql(string sql, string columns, string order, string count = null)
        {
            if (string.IsNullOrWhiteSpace(order))
            {
                throw new SpearException("need order by!");
            }

            count = string.IsNullOrWhiteSpace(count) ? "COUNT(1)" : count;

            var countSql = sql.Replace(columns, $"{count} ").Replace($" {order}", string.Empty);
            if (countSql.IsMatch("group by", RegexOptions.IgnoreCase))
                countSql = $"SELECT {count} FROM ({countSql}) AS count_t";

            sql = sql.Replace($" {order}", string.Empty);
            sql = sql.Replace(columns, $"ROW_NUMBER() OVER({order}) AS [paged_row],{columns}");

            sql =
                $"SELECT * FROM ({sql}) [_proj] WHERE [paged_row] BETWEEN (@skip +1) AND (@skip+@size);{countSql};";
            return sql;
        }

        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return SqlClientFactory.Instance.CreateConnection();
        }
    }
}
