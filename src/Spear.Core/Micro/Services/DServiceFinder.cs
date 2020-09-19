using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Spear.Core.Micro.Services
{
    public abstract class DServiceFinder : DServiceRoute, IServiceFinder
    {
        private readonly IMemoryCache _cache;
        protected readonly ILogger Logger;
        protected DServiceFinder(IMemoryCache memoryCache, ILogger logger)
        {
            _cache = memoryCache;
            Logger = logger;
        }

        /// <summary> 查询服务 </summary>
        /// <param name="serviceType"></param>
        /// <param name="modes"></param>
        /// <returns></returns>
        protected abstract Task<List<ServiceAddress>> QueryService(Type serviceType, ProductMode[] modes);

        /// <summary> 服务发现 </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public async Task<List<ServiceAddress>> Find(Type serviceType)
        {
            var key = serviceType.Assembly.ServiceName();
            var modes = new List<ProductMode> { Constants.Mode };
            if (Constants.Mode == ProductMode.Dev)
                modes.Add(ProductMode.Test);

            if (_cache == null)
            {
                return await QueryService(serviceType, modes.ToArray());
            }

            if (_cache.TryGetValue<List<ServiceAddress>>(key, out var services))
                return services;            
            services = await QueryService(serviceType, modes.ToArray());
            if (services != null && services.Any())
            {
                Logger?.LogInformation($"找到{services.Count}个服务,{string.Join(";", services)}");
                _cache.Set(key, services, TimeSpan.FromMinutes(2));
            }
            else
            {
                Logger?.LogInformation($"未找到相关服务:{key}");
            }

            return services;
        }

        public Task CleanCache(Type serviceType)
        {
            var key = serviceType.Assembly.ServiceName();
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
