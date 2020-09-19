using Spear.Core.Data;
using Spear.Core.Extensions;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Spear.Dapper.Mysql
{
    public class MySqlConnectionAdapter : IDbConnectionAdapter
    {
        /// <summary> 适配器名称 </summary>
        public string ProviderName => "MySql";
        /// <summary> 连接类型 </summary>
        public Type ConnectionType => typeof(MySqlConnection);

        /// <summary> 格式化SQL </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string FormatSql(string sql)
        {
            return sql.Replace("[", "`").Replace("]", "`");
        }

        /// <summary> 构造分页SQL </summary>
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
                $"{sql} LIMIT @skip,@size;{countSql};";
            return sql;
        }

        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        public IDbConnection Create()
        {
            return MySqlClientFactory.Instance.CreateConnection();
        }
    }
}
