using Spear.Core.Data;
using Spear.Core.Extensions;
using Npgsql;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Spear.Dapper.PostgreSql
{
    public class PostgreSqlAdapter : IDbConnectionAdapter
    {
        /// <summary> 适配器名称 </summary>
        public string ProviderName => "PostgreSql";

        /// <summary> 连接类型 </summary>
        public Type ConnectionType => typeof(NpgsqlConnection);

        /// <summary> SQL格式化 </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string FormatSql(string sql)
        {
            //:In => ANY
            return sql.Replace("@", ":").Replace("?", ":").Replace("[", "\"").Replace("]", "\"");
        }

        /// <summary> 构建分页SQL </summary>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string PageSql(string sql, string columns, string order, string count = null)
        {
            count = string.IsNullOrWhiteSpace(count) ? "COUNT(1)" : count;
            var countSql = sql.Replace(columns, $"{count} ");
            if (order.IsNotNullOrEmpty())
            {
                countSql = countSql.Replace($" {order}", string.Empty);
            }
            if (countSql.IsMatch("group by", RegexOptions.IgnoreCase))
                countSql = $"SELECT {count} FROM ({countSql}) AS count_t";
            sql =
                $"{sql} LIMIT @size OFFSET @skip;{countSql};";
            return sql;
        }

        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return NpgsqlFactory.Instance.CreateConnection();
        }
    }
}
