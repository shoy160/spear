using Acb.Core;
using Acb.Core.Cache;
using Acb.Core.Domain;
using Acb.Core.Extensions;
using Consul;
using Spear.Core.Micro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acb.Core.Serialize;

namespace Spear.Consul
{
    public class ConsulServiceFinder : IServiceFinder
    {
        private readonly ICache _cache;
        private readonly string _consulServer;
        private readonly string _consulToken;

        public ConsulServiceFinder(string server, string token = null)
        {
            _cache = CacheManager.GetCacher<ConsulServiceFinder>();
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
                var list = await client.Catalog.Service(name.Name, $"{Consts.Mode}");
                var items = list.Response.Select(t =>
                {
                    if (t.ServiceMeta.TryGetValue("serverAddress", out var json))
                        return JsonHelper.Json<ServiceAddress>(json);
                    return new ServiceAddress(t.Address, t.ServicePort);
                }).ToArray();
                services.AddRange(items);
                //开发环境 可调用测试环境的微服务
                if (Consts.Mode == ProductMode.Dev)
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
