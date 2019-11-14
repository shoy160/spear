using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Nacos.Sdk.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public void UpdateCache(ConfigRequest request, string config)
        {
            var key = BuildKey(request);
            _configCache[key] = config;
        }

        private static string BuildKey(ConfigRequest request)
        {
            return $"{request.DataId}_{request.Group}_{request.Tenant}";
        }

        private async Task PollingAsync(AddListenerRequest request, long timeout)
        {
            try
            {
                var key = BuildKey(request);
                if (_configCache.TryGetValue(key, out var config))
                {
                    request.Content = config;
                }
                if (string.IsNullOrWhiteSpace(request.Content))
                    return;

                var result = await _client.AddListenerAsync(request, timeout);
                if (string.IsNullOrWhiteSpace(result))
                    return;
                if (_listeners.TryGetValue(key, out var listener))
                {
                    foreach (var callback in listener.Callbacks)
                    {
                        callback.Invoke(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"配置监听异常：{ex.Message}");
            }
        }

        private async Task OnTimerElapsed(object sender)
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
                await PollingAsync(request, timeout);
                timer.Change(TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);
            }
        }

        public void Add(AddListenerRequest request, Action<string> callback, long interval = 120, long timeout = 30000)
        {
            var key = BuildKey(request);
            if (_listeners.TryGetValue(key, out var listener))
            {
                listener.Callbacks.Add(callback);
                return;
            }

            var timer = new Timer(async sender => { await OnTimerElapsed(sender); },
                new object[] { request, interval, timeout },
                TimeSpan.FromSeconds(interval), Timeout.InfiniteTimeSpan);

            listener = new NacosListener(timer)
            {
                Callbacks = new List<Action<string>> { callback }
            };
            _listeners.TryAdd(key, listener);
        }

        public void Remove(AddListenerRequest request)
        {
            var key = BuildKey(request);
            if (_listeners.TryGetValue(key, out var listener))
            {
                listener.Timer.Dispose();
                listener.Timer = null;
            }
        }
    }
}
