using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace Spear.Core.Data
{
    /// <summary> 数据库连接管理器 </summary>
    public static class DbConnectionManager
    {
        private static readonly ConcurrentDictionary<string, Lazy<IDbConnectionAdapter>> Adapters;
        private static ILogger Logger => CurrentIocManager.CreateLogger(typeof(DbConnectionManager));

        static DbConnectionManager()
        {
            Adapters = new ConcurrentDictionary<string, Lazy<IDbConnectionAdapter>>();
        }

        /// <summary> 添加适配器 </summary>
        /// <param name="adapter"></param>
        public static void AddAdapter(IDbConnectionAdapter adapter)
        {
            if (adapter == null || string.IsNullOrWhiteSpace(adapter.ProviderName))
                return;
            var key = adapter.ProviderName.ToLower();
            if (Adapters.ContainsKey(key))
                return;
            Adapters.TryAdd(key, new Lazy<IDbConnectionAdapter>(() => adapter));
        }

        /// <summary> 创建数据库适配器 </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static IDbConnectionAdapter Create(string providerName)
        {
            if (Adapters.TryGetValue(providerName.ToLower(), out var adapter))
                return adapter.Value;
            throw new SpearException($"不支持的DbProvider：{providerName}");
        }

        /// <summary> 格式化SQL </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string FormatSql(this IDbConnection conn, string sql)
        {
            foreach (var lazyAdapter in Adapters.Values)
            {
                var adapter = lazyAdapter.Value;
                if (adapter.ConnectionType != conn.GetType())
                    continue;
                sql = adapter.FormatSql(sql);
                if (Logger != null && Logger.IsEnabled(LogLevel.Debug))
                    Logger.LogDebug(sql);
                return sql;
            }
            return sql;
        }

        /// <summary> 生成分页SQL </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <param name="formatSql">格式化SQL</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string PagedSql(this IDbConnection conn, string sql, string columns, string order, bool formatSql = true, string count = null)
        {
            foreach (var lazyAdapter in Adapters.Values)
            {
                var adapter = lazyAdapter.Value;
                if (adapter.ConnectionType == conn.GetType())
                {
                    sql = adapter.PageSql(sql, columns, order, count);
                    return formatSql ? adapter.FormatSql(sql) : sql;
                }
            }

            return sql;
        }
    }
}
