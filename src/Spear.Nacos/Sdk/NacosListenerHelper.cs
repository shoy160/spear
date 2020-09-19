using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Extensions;
using Spear.Nacos.Sdk.Requests.Config;
using Spear.Nacos.Sdk.Requests.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Nacos.Sdk
{
    public class NacosListener
    {
        public Timer Timer { get; internal set; }

        public List<Action<string>> Callbacks { get; set; }

        public NacosListener(Timer timer)
        {
            Timer = timer;
            Callbacks = new List<Action<string>>();
        }

    }
    public class NacosListenerHelper
    {
        private readonly INacosClient _client;
        private readonly ConcurrentDictionary<string, NacosListener> _listeners;
        private readonly IDictionary<string, string> _configCache;
        private readonly ILogger<NacosListenerHelper> _logger;

        public NacosListenerHelper(INacosClient client, ILoggerFactory loggerFactory)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<NacosListenerHelper>();
            _listeners = new ConcurrentDictionary<string, NacosListener>();
            _configCache = new Dictionary<string, string>();
        }

        #region config
        public void UpdateCache(ConfigRequest request, string config)
        {
            var key = BuildKey(request);
            _configCache[key] = config;
        }

        private static string BuildKey(ConfigRequest request)
        {
            return $"{request.DataId}_{request.Group}_{request.Tenant}";
        }

        private async Task<string> PollingAsync(AddListenerRequest request, long timeout)
        {
            try
            {
                var key = BuildKey(request);
                if (_configCache.TryGetValue(key, out var config))
                {
                    request.Content = config;
                }
                if (string.IsNullOrWhiteSpace(request.Content))
                    return string.Empty;
                _logger.LogDebug($"添加日志长轮询:{key}");
                return await _client.AddListenerAsync(request, timeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"配置监听异常：{ex.Message}");
                return string.Empty;
            }
        }

        private async Task ConfigPolling(object sender)
        {
            if (!(sender is object[] param) || param.Length != 3)
                return;
            var request = (AddListenerRequest)param[0];
            var interval = param[1].CastTo(120);
            var timeout = param[2].CastTo(30000);
            var key = BuildKey(request);
            if (_listeners.TryGetValue(key, out var listener))
            {
                var timer = listener.Timer;
                timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                var result = await PollingAsync(request, timeout);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    foreach (var callback in listener.Callbacks)
                    {
                        callback.Invoke(result);
                    }
                }

                timer.Change(TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);
            }
        }

        public string AddConfig(AddListenerRequest request, Action<string> callback, int interval = 120, int timeout = 30000)
        {
            var key = BuildKey(request);
            if (_listeners.TryGetValue(key, out var listener))
            {
                listener.Callbacks.Add(callback);
                return key;
            }

            var timer = new Timer(async sender => { await ConfigPolling(sender); },
                new object[] { request, interval, timeout },
                TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);

            listener = new NacosListener(timer)
            {
                Callbacks = new List<Action<string>> { callback }
            };
            _listeners.TryAdd(key, listener);
            return key;
        }
        #endregion

        private async Task<string> SendBeat(CreateInstanceRequest request)
        {
            if (request == null) return string.Empty;
            try
            {
                _logger.LogDebug($"发送心跳包：{request.serviceName},{request.ip}:{request.port}");
                var beat = new InstanceBeat
                {
                    serviceName = request.serviceName,
                    ip = request.ip,
                    port = request.port,
                    weight = request.weight,
                    cluster = string.Empty,
                    metadata = request.Meta,
                    scheduled = true
                };
                return await _client.InstanceBeat(new InstanceBeatRequest
                {
                    namespaceId = request.namespaceId,
                    serviceName = request.serviceName,
                    groupName = request.groupName,
                    ephemeral = request.ephemeral,
                    beat = JsonConvert.SerializeObject(beat)
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"心跳包发送异常：{ex.Message}");
                return string.Empty;
            }
        }

        private async Task ConfigBeat(object sender)
        {
            if (!(sender is object[] param) || param.Length != 3)
                return;
            var key = param[0].ToString();
            var request = param[1] as CreateInstanceRequest;
            var interval = param[2].CastTo(5);
            if (_listeners.TryGetValue(key, out var listener))
            {
                var timer = listener.Timer;
                timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                var result = await SendBeat(request);
                if (listener.Callbacks.Any())
                {
                    foreach (var callback in listener.Callbacks)
                    {
                        callback(result);
                    }
                }
                timer.Change(TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary> 添加心跳包发送器 </summary>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <param name="interval">间隔(秒，默认：5)</param>
        /// <returns></returns>
        public string AddServiceBeat(CreateInstanceRequest request, Action<string> callback, int interval = 5)
        {
            var key = JsonConvert.SerializeObject(request).Md5();
            if (_listeners.TryGetValue(key, out var listener))
            {
                listener.Callbacks.Add(callback);
                return key;
            }

            var timer = new Timer(async sender => await ConfigBeat(sender), new object[] { key, request, interval },
                TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);
            listener = new NacosListener(timer)
            {
                Callbacks = new List<Action<string>> { callback }
            };
            _listeners.TryAdd(key, listener);
            return key;
        }

        public void RemoveListener(string key)
        {
            if (!_listeners.TryGetValue(key, out var listener))
                return;
            listener.Timer.Dispose();
            listener.Timer = null;
            _listeners.TryRemove(key, out _);
        }

        public void Clean()
        {
            foreach (var item in _listeners)
            {
                item.Value.Timer?.Dispose();
                item.Value.Timer = null;
            }
            _listeners.Clear();
        }
    }
}
