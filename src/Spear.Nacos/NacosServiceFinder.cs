using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core;
using Spear.Core.Micro.Services;
using Spear.Nacos.Sdk;
using Spear.Nacos.Sdk.Requests.Service;

namespace Spear.Nacos
{
    public class NacosServiceFinder : DServiceFinder
    {
        private readonly NacosConfig _config;
        private readonly INacosClient _client;
        private readonly ILogger<NacosServiceFinder> _logger;

        public NacosServiceFinder(NacosConfig config, INacosClient client, ILogger<NacosServiceFinder> logger, IMemoryCache cache)
            : base(cache, logger)
        {
            _config = config;
            _client = client;
            _logger = logger;
        }

        protected override async Task<List<ServiceAddress>> QueryService(Type serviceType, ProductMode[] modes)
        {
            var ass = serviceType.Assembly;
            var services = new List<ServiceAddress>();
            var request = new InstanceListRequest
            {
                namespaceId = _config.Tenant,
                serviceName = ass.ServiceName(),
                groupName = _config.Group,
                clusters = string.Empty,
                healthyOnly = true
            };
            var results = await _client.InstanceList(request);
            if (results?.Hosts == null || !results.Hosts.Any())
                return services;
            foreach (var host in results.Hosts)
            {
                var mate = host.Metadata;
                if (mate.TryGetValue(KeyMode, out var serviceMode))
                {
                    var m = serviceMode.CastTo(ProductMode.Dev);
                    if (!modes.Contains(m))
                        continue;
                }

                services.Add(mate.TryGetValue(KeyService, out var service)
                    ? JsonConvert.DeserializeObject<ServiceAddress>(service)
                    : new ServiceAddress(host.Ip, host.Port));
            }

            return services;
        }
    }
}
