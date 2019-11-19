using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    public abstract class DServiceFinder : DServiceRoute, IServiceFinder
    {
        private readonly IMemoryCache _cache;
        protected DServiceFinder(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
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
            if (_cache.TryGetValue<List<ServiceAddress>>(key, out var services))
                return services;
            var modes = new List<ProductMode> { Constants.Mode };
            if (Constants.Mode == ProductMode.Dev)
                modes.Add(ProductMode.Test);
            services = await QueryService(serviceType, modes.ToArray());
            if (services != null && services.Any())
                _cache.Set(key, services, TimeSpan.FromMinutes(2));
            return services;
        }
    }
}
