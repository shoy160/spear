using Consul;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Spear.Core;
using Spear.Core.Micro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spear.Consul
{
    public class ConsulServiceFinder : DServiceFinder
    {
        private readonly string _consulServer;
        private readonly string _consulToken;

        public ConsulServiceFinder(string server, string token = null)
        {
            _consulServer = server;
            _consulToken = token;
        }

        private IConsulClient CreateClient()
        {
            return new ConsulClient(cfg =>
            {
                cfg.Address = new Uri(_consulServer);
                if (!string.IsNullOrWhiteSpace(_consulToken))
                    cfg.Token = _consulToken;
            });
        }

        protected override async Task<List<ServiceAddress>> QueryService(Type serviceType, ProductMode[] modes)
        {
            var ass = serviceType.Assembly;
            var services = new List<ServiceAddress>();
            using (var client = CreateClient())
            {
                var list = await client.Catalog.Service(ass.ServiceName());
                foreach (var service in list.Response)
                {
                    if (service.ServiceMeta.TryGetValue(KeyMode, out var modeValue))
                    {
                        var mode = modeValue.CastTo(ProductMode.Dev);
                        if (!modes.Contains(mode))
                            continue;
                    }

                    services.Add(service.ServiceMeta.TryGetValue(KeyService, out var json)
                        ? JsonConvert.DeserializeObject<ServiceAddress>(json)
                        : new ServiceAddress(service.Address, service.ServicePort));
                }

                return services;
            }
        }
    }
}
