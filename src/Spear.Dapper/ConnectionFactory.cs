using Spear.Core.Data;
using Spear.Core.Data.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;
using Spear.Core.Dependency;

namespace Spear.Dapper
{
    /// <summary> 数据库连接管理 </summary>
    public class ConnectionFactory : ISingleDependency
    {
        private readonly ConcurrentDictionary<Thread, Lazy<Dictionary<string, ConnectionStruct>>> _connectionCache;
        private static readonly object SyncObj = new object();
        private int _createCount;
        private int _removeCount;
        private int _cacheCount;
        private int _clearCount;
        private readonly Timer _clearTimer;
        private bool _clearTimerRun;
        private readonly ILogger _logger;

        public ConnectionFactory(ILogger<ConnectionFactory> logger)
        {
            _connectionCache = new ConcurrentDictionary<Thread, Lazy<Dictionary<string, ConnectionStruct>>>();
            _logger = logger;
            //配置文件改变时，清空缓存
            //ConfigHelper.Instance.ConfigChanged += name =>
            //{
            //    _connectionCache.Clear();
            //};
            _clearTimer = new Timer(1000 * 60);
            _clearTimer.Elapsed += ClearTimerElapsed;
            _clearTimer.Enabled = true;
            _clearTimer.Stop();
            _clearTimerRun = false;
        }

        private void ClearTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _clearCount++;
            ClearDict();
            if (_connectionCache.Count == 0)
            {
                _clearTimerRun = false;
                _clearTimer.Stop();
            }
        }

        /// <summary> 清理失效的线程级缓存 </summary>
        private void ClearDict()
        {
            if (_connectionCache.Count == 0)
                return;
            foreach (var key in _connectionCache.Keys)
            {
                if (!_connectionCache.TryGetValue(key, out var lazyDict))
                    continue;
                var connDict = lazyDict.Value;
                foreach (var name in connDict.Keys)
                {
                    if (key.IsAlive && connDict[name].IsAlive())
                        continue;
                    var conn = connDict[name];
                    if (connDict.Remove(name))
                    {
                        _removeCount++;
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"Connection Dispose:{conn}");
                        conn?.Dispose();
                    }
                }

                if (connDict.Count == 0)
                    _connectionCache.TryRemove(key, out _);
            }
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(ToString());
        }

        private IDbConnection CreateConnection(string providerName, string connectionString)
        {
            //新开连接
            _createCount++;
            var adapter = DbConnectionManager.Create(providerName);
            var connection = adapter.Create();
            if (connection == null)
                throw new Exception("创建数据库连接失败");
            connection.ConnectionString = connectionString;
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Create Connection: {connectionString}");
            return connection;
        }


        private static string Md5(string inputString)
        {
            var algorithm = MD5.Create();
            algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            return BitConverter.ToString(algorithm.Hash).Replace("-", "").ToUpper();
        }
        /// <summary> 创建数据库连接 </summary>
        /// <param name="config"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        private IDbConnection GetConnection(ConnectionConfig config, bool fromCache = true)
        {
            lock (SyncObj)
            {
                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                    throw new ArgumentException($"未找到的数据库配置[{config?.Name}]");

                if (!fromCache)
                {
                    return CreateConnection(config.ProviderName, config.ConnectionString);
                }

                var key = Md5(config.ConnectionString);
                var connectionKey = Thread.CurrentThread;
                Dictionary<string, ConnectionStruct> connDict;
                if (_connectionCache.TryGetValue(connectionKey, out var lazyDict))
                {
                    connDict = lazyDict.Value;
                }
                else
                {
                    connDict = new Dictionary<string, ConnectionStruct>();
                    if (!_connectionCache.TryAdd(connectionKey,
                        new Lazy<Dictionary<string, ConnectionStruct>>(() => connDict)))
                    {
                        throw new Exception("Can not set db connection!");
                    }
                }
                if (connDict.ContainsKey(key))
                {
                    _cacheCount++;
                    return connDict[key].GetConnection();
                }
                //新开连接
                var connection = CreateConnection(config.ProviderName, config.ConnectionString);

                connDict.Add(key, new ConnectionStruct(connection));

                if (_clearTimerRun) return connection;
                //开启定时清理任务
                _clearTimer.Start();
                _clearTimerRun = true;
                return connection;
            }
        }


        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        /// <param name="fromCache">是否启用线程缓存</param>
        /// <returns></returns>
        public IDbConnection Connection(string connectionName = null, bool fromCache = true)
        {
            var config = ConnectionConfig.Config(connectionName) ?? new ConnectionConfig { Name = connectionName };
            return GetConnection(config, fromCache);
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        /// <param name="fromCache">是否启用线程缓存</param>
        /// <returns></returns>
        public IDbConnection Connection(Enum connectionName, bool fromCache = true)
        {
            return Connection(connectionName.ToString(), fromCache);
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public IDbConnection Connection(string connectionString, string provider, bool fromCache = true)
        {
            return GetConnection(new ConnectionConfig { Name = connectionString, ProviderName = provider, ConnectionString = connectionString },
                fromCache);
        }

        /// <summary> 缓存总数/// </summary>
        public int Count => _connectionCache.Sum(t => t.Value.Value.Count);

        /// <summary> 连接缓存信息 </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var proc = Process.GetCurrentProcess();
            sb.AppendLine($"专用工作集内存：{proc.PrivateMemorySize64 / 1024.0}kb");
            sb.AppendLine($"工作集内存：{proc.WorkingSet64 / 1024.0}kb");
            sb.AppendLine($"最大内存：{proc.PeakWorkingSet64 / 1024.0}kb");
            sb.AppendLine($"线程数：{proc.Threads.Count}");
            foreach (var connectionStruct in _connectionCache)
            {
                foreach (var item in connectionStruct.Value.Value)
                {
                    sb.AppendLine(item.ToString());
                }
            }
            sb.AppendLine($"create:{_createCount},total:{Count},useCache:{_cacheCount},clear:{_clearCount},remove:{_removeCount}");
            return sb.ToString();
        }
    }
}
