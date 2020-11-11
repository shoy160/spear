using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using Spear.Core.Helper;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Spear.Redis
{
    /// <summary> Redis管理器 </summary>
    public class RedisManager : IDisposable
    {
        private readonly string _managerId;

        private const string CacheSyncChannel = "cache.manager";
        //缓存同步
        private bool _cacheSync;

        private ILogger<RedisManager> _managerLogger;
        private ILogger Logger
        {
            get
            {
                if (_managerLogger != null) return _managerLogger;
                var factory = CurrentIocManager.Resolve<ILoggerFactory>();
                return _managerLogger = factory.CreateLogger<RedisManager>();
            }
        }

        private readonly ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> _connections;

        private RedisManager()
        {
            _managerId = IdentityHelper.Guid16;
            _connections = new ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>>();

            ConfigHelper.Instance.ConfigChanged += obj =>
            {
                if (_connections.Count <= 0)
                    return;
                _connections.Values.Foreach(t => t.Value.Close());
                _connections.Clear();
            };
        }

        /// <summary> 单例模式 </summary>
        public static RedisManager Instance => Singleton<RedisManager>.Instance ??
                                               (Singleton<RedisManager>.Instance = new RedisManager());

        private static RedisConfig GetConfig(string configName)
        {
            var config = RedisConfig.Config(configName);
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new ArgumentException($"Redis:{config.Name}配置异常", nameof(configName));
            return config;
        }

        public ConnectionMultiplexer Connect(string connectionString)
        {
            var opts = ConfigurationOptions.Parse(connectionString);
            return Connect(opts);
        }

        private static IEnumerable<EndPoint> ParseEndPoint(IEnumerable<KeyValuePair<string, string>[]> nodes)
        {
            var endpoints = new List<EndPoint>();
            foreach (var slave in nodes)
            {
                var address = slave.Single(i => i.Key == "ip").Value;
                var port = slave.Single(i => i.Key == "port").Value;
                endpoints.Add(new IPEndPoint(IPAddress.Parse(address), port.CastTo(6379)));
            }
            return endpoints.ToArray();
        }

        /// <summary> 获取哨兵模式所有服务节点 </summary>
        /// <param name="sentinelConn"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static EndPoint[] GetAllRedisServersFromSentinel(IConnectionMultiplexer sentinelConn, string serviceName)
        {
            var endpoints = new List<EndPoint>();
            foreach (var endPoint in sentinelConn.GetEndPoints())
            {
                var server = sentinelConn.GetServer(endPoint);
                if (!server.IsConnected)
                    continue;
                //master
                var masters = server.SentinelMasters();
                endpoints.AddRange(ParseEndPoint(masters));
                //slaves
                var slaves = server.SentinelSlaves(serviceName);
                endpoints.AddRange(ParseEndPoint(slaves));
            }
            return endpoints.Distinct().ToArray();
        }

        public ConnectionMultiplexer Connect(ConfigurationOptions configOpts)
        {
            ConnectionMultiplexer conn;

            if (string.IsNullOrWhiteSpace(configOpts.ServiceName))
            {
                var points = string.Join<EndPoint>(",", configOpts.EndPoints.ToArray());
                Logger.LogInformation($"Create Redis: {points},db:{configOpts.DefaultDatabase}");
                conn = ConnectionMultiplexer.Connect(configOpts);
            }
            else
            {
                var clusterConfig = configOpts.Clone();
                clusterConfig.EndPoints.Clear();

                //哨兵模式
                configOpts.TieBreaker = string.Empty;
                configOpts.Password = string.Empty;
                configOpts.CommandMap = CommandMap.Sentinel;

                using (var sentinelConn = ConnectionMultiplexer.Connect(configOpts))
                {
                    //订阅哨兵事件
                    sentinelConn.GetSubscriber().Subscribe("+reset-master", (channel, value) =>
                     {
                         _connections.Clear();
                     });
                    var endpoints = GetAllRedisServersFromSentinel(sentinelConn, configOpts.ServiceName);
                    foreach (var endpoint in endpoints)
                    {
                        clusterConfig.EndPoints.Add(endpoint);
                    }
                    var points = string.Join<EndPoint>(",", endpoints);
                    Logger.LogInformation($"Create Redis: {points},db:{configOpts.DefaultDatabase}");
                    conn = ConnectionMultiplexer.Connect(clusterConfig);
                }
            }

            conn.ConfigurationChanged += (sender, e) =>
            {
                Logger.LogDebug($"Redis Configuration changed: {e.EndPoint}");
            };
            conn.ConnectionRestored += (sender, e) => { Logger.LogDebug($"Redis ConnectionRestored: {e.EndPoint}"); };
            conn.ErrorMessage += (sender, e) => { Logger.LogError($"Redis Error{e.EndPoint}: {e.Message}"); };
            conn.ConnectionFailed += (sender, e) =>
            {
                Logger.LogWarning(
                    $"Redis 重新连接：Endpoint failed: ${e.EndPoint}, ${e.FailureType},${e.Exception?.Message}");
            };
            conn.InternalError += (sender, e) => { Logger.LogWarning($"Redis InternalError:{e.Exception.Message}"); };
            return conn;
        }

        public ConnectionMultiplexer GetConnection(string configName)
        {
            var config = GetConfig(configName);
            var opts = ConfigurationOptions.Parse(config.ConnectionString);
            return GetConnection(config.Name, opts);
        }

        private ConnectionMultiplexer GetConnection(string configName, ConfigurationOptions configOpts)
        {
            if (_connections.TryGetValue(configName, out var lazyConn))
            {
                if (lazyConn.Value.IsConnected)
                    return lazyConn.Value;
                lazyConn.Value.Dispose();
            }
            var conn = new Lazy<ConnectionMultiplexer>(() => Connect(configOpts));
            _connections[configName] = conn;
            return conn.Value;
        }

        public IDatabase GetDatabase(RedisConfig config, int defaultDb = -1)
        {
            if (string.IsNullOrWhiteSpace(config.Name))
                return Connect(config.ConnectionString).GetDatabase();
            return GetConnection(config.Name, ConfigurationOptions.Parse(config.ConnectionString))
                .GetDatabase(defaultDb);
        }

        /// <summary> 获取Database </summary>
        /// <param name="configName"></param>
        /// <param name="defaultDb"></param>
        /// <returns></returns>
        public IDatabase GetDatabase(string configName = null, int defaultDb = -1)
        {
            var conn = GetConnection(configName);
            return conn.GetDatabase(defaultDb);
        }

        /// <summary> 获取Server </summary>
        /// <param name="configName">配置名称</param>
        /// <param name="endPointsIndex"></param>
        /// <returns></returns>
        public IServer GetServer(string configName = null, int endPointsIndex = 0)
        {
            var config = GetConfig(configName);
            var confOption = ConfigurationOptions.Parse(config.ConnectionString);
            return GetConnection(config.Name, confOption).GetServer(confOption.EndPoints[endPointsIndex]);
        }

        /// <summary> 获取Server </summary>
        /// <param name="configName">配置名称</param>
        /// <param name="host">主机名</param>
        /// <param name="port">端口</param>
        /// <returns></returns>
        public IServer GetServer(string configName, string host, int port)
        {
            var conn = GetConnection(configName);
            return conn.GetServer(host, port);
        }

        public IServer GetServer(RedisConfig config, int endPointsIndex = 0)
        {
            var confOption = ConfigurationOptions.Parse(config.ConnectionString);
            if (string.IsNullOrWhiteSpace(config.Name))
                return Connect(confOption).GetServer(confOption.EndPoints[endPointsIndex]);
            return GetConnection(config.Name, confOption).GetServer(confOption.EndPoints[endPointsIndex]);
        }

        /// <summary> 获取订阅 </summary>
        /// <param name="configName">配置名称</param>
        /// <returns></returns>
        public ISubscriber GetSubscriber(string configName = null)
        {
            return GetConnection(configName).GetSubscriber();
        }

        /// <summary> 释放资源 </summary>
        public void Dispose()
        {
            if (_connections == null || _connections.Count == 0)
                return;
            _connections.Values.Foreach(t => t.Value.Close());
        }

        private void ConfigSubscribe(RedisChannel channel, RedisValue val)
        {
            var arr = val.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3 || arr[0] == _managerId) return;
            Logger.LogDebug($"subscribe:{val}");
            //var cache = CacheManager.GetCacher(arr[1], cache: false);
            //cache.Remove(arr[2]);
        }

        internal void CacheSubscribe(string configName = null)
        {
            _cacheSync = true;
            Logger.LogInformation("开启两级缓存同步");
            //订阅
            var sub = GetSubscriber(configName);
            sub.SubscribeAsync(CacheSyncChannel, ConfigSubscribe);
        }

        internal void CachePublish(string region, string key, string configName = null)
        {
            if (!_cacheSync || string.IsNullOrWhiteSpace(key)) return;
            var sub = GetSubscriber(configName);
            Logger.LogInformation($"publish cache:{region},{key}");
            sub.PublishAsync(CacheSyncChannel, $"{_managerId},{region},{key}");
        }

        internal void CachePublish(string region, IEnumerable<string> keys, string configName = null)
        {
            foreach (var key in keys)
            {
                CachePublish(region, key);
            }
        }
    }
}
