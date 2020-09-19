using Spear.Core.Data;
using Spear.Core.Extensions;
using System;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace Spear.Dapper.SQLite
{
    public class SqliteConnectionAdapter : IDbConnectionAdapter
    {
        public string ProviderName => "SQLite";
        public Type ConnectionType => typeof(SQLiteConnection);

        public string FormatSql(string sql)
        {
            return sql;
        }

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

        public IDbConnection Create()
        {
            return new SQLiteConnection();
        }
    }
}
