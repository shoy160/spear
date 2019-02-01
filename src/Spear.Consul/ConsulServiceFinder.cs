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
    public class ConsulServiceFinder : IServiceFinder
    {
        private readonly IMemoryCache _cache;
        private readonly string _consulServer;
        private readonly string _consulToken;

        public ConsulServiceFinder(IMemoryCache cache, string server, string token = null)
        {
            _cache = cache;
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

        public async Task<List<ServiceAddress>> Find(Type serviceType)
        {
            var ass = serviceType.Assembly;
            var key = ass.AssemblyKey();
            var services = _cache.Get<List<ServiceAddress>>(key);
            if (services != null)
                return services;
            var name = ass.GetName();
            services = new List<ServiceAddress>();
            using (var client = CreateClient())
            {
                var list = await client.Catalog.Service(name.Name, $"{Constants.Mode}");
                var items = list.Response.Select(t =>
                {
                    if (t.ServiceMeta.TryGetValue("serverAddress", out var json))
                        return JsonConvert.DeserializeObject<ServiceAddress>(json);
                    return new ServiceAddress(t.Address, t.ServicePort);
                }).ToArray();
                services.AddRange(items);
                //开发环境 可调用测试环境的微服务
                if (Constants.Mode == ProductMode.Dev)
                {
                    list = client.Catalog.Service(name.Name, $"{ProductMode.Test}").Result;
                    items = list.Response.Select(t => new ServiceAddress(t.ServiceAddress, t.ServicePort)).ToArray();
                    services.AddRange(items);
                }
            }

            _cache.Set(key, services, TimeSpan.FromMinutes(5));
            return services;
        }
    }
}
