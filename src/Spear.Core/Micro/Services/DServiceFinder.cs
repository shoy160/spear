using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    public abstract class DServiceFinder : DServiceRoute, IServiceFinder
    {
        private readonly ConcurrentDictionary<string, List<ServiceAddress>> _serviceCache;

        protected DServiceFinder()
        {
            _serviceCache = new ConcurrentDictionary<string, List<ServiceAddress>>();
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
            if (_serviceCache.TryGetValue(key, out var services))
            {
                return services;
            }
            var modes = new List<ProductMode> { Constants.Mode };
            if (Constants.Mode == ProductMode.Dev)
                modes.Add(ProductMode.Test);
            services = await QueryService(serviceType, modes.ToArray());
            if (services != null && services.Any())
                _serviceCache.TryAdd(key, services);
            return services;
        }
    }
}
