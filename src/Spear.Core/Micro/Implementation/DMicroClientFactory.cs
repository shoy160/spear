using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Micro.Services;

namespace Spear.Core.Micro.Implementation
{
    public abstract class DMicroClientFactory : IMicroClientFactory, IDisposable
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected readonly ILogger Logger;
        protected readonly IMessageCodecFactory CodecFactory;
        protected readonly IMicroExecutor MicroExecutor;

        private readonly ConcurrentDictionary<ServiceAddress, Lazy<Task<IMicroClient>>> _clients;

        protected DMicroClientFactory(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory, IMicroExecutor microExecutor = null)
        {
            CodecFactory = codecFactory;
            MicroExecutor = microExecutor;
            LoggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger(GetType());
            _clients = new ConcurrentDictionary<ServiceAddress, Lazy<Task<IMicroClient>>>();
        }

        /// <summary> 创建客户端 </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected abstract Task<IMicroClient> Create(ServiceAddress address);

        /// <summary> 删除客户端缓存 </summary>
        /// <param name="address"></param>
        protected void Remove(ServiceAddress address)
        {
            Logger.LogInformation($"移除客户端:{address}");
            _clients.TryRemove(address, out _);
        }

        public Task<IMicroClient> CreateClient(ServiceAddress serviceAddress)
        {
            try
            {
                var lazyClient = _clients.GetOrAdd(serviceAddress,
                    key => new Lazy<Task<IMicroClient>>(async () =>
                    {
                        Logger.LogInformation($"创建客户端：{key}创建客户端。");
                        return await Create(key);
                    }));
                return lazyClient.Value;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"创建客户端失败:{serviceAddress}");
                Remove(serviceAddress);
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }
    }
}
