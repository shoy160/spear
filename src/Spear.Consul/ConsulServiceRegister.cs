using Acb.Core;
using Acb.Core.Extensions;
using Acb.Core.Logging;
using Consul;
using Spear.Core.Micro.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Consul
{
    public class ConsulServiceRegister : IServiceRegister
    {
        private readonly string _consulServer;
        private readonly string _consulToken;
        private readonly List<string> _services;
        private readonly ILogger _logger;

        public ConsulServiceRegister(string server, string token)
        {
            _consulServer = server;
            _consulToken = token;
            _logger = LogManager.Logger<ConsulServiceRegister>();
            _services = new List<string>();
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

        public async Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress)
        {
            using (var client = CreateClient())
            {
                foreach (var ass in assemblyList)
                {
                    var assName = ass.GetName();
                    var service = new AgentServiceRegistration
                    {
                        ID = $"{ass.GetName().Name}_{serverAddress}".Md5(),
                        Name = assName.Name,
                        Tags = new[] { $"{Consts.Mode}" },
                        EnableTagOverride = true,
                        Address = serverAddress.Address(),
                        Port = serverAddress.Port,
                        Meta = new Dictionary<string, string>
                        {
                            {"serverAddress", serverAddress.ToJson()}
                        }
                    };
                    _services.Add(service.ID);
                    var result = await client.Agent.ServiceRegister(service);
                    if (result.StatusCode != HttpStatusCode.OK)
                        _logger.Warn(
                            $"服务注册失败 [{assName.Name},{serverAddress}]:{result.StatusCode},{result.RequestTime}");
                    else
                        _logger.Info($"服务注册成功 [{assName.Name},{serverAddress}]");
                }
            }
        }

        public async Task Deregist()
        {
            using (var client = CreateClient())
            {
                foreach (var service in _services)
                {
                    await client.Agent.ServiceDeregister(service);
                    _logger.Info($"注销服务 [{service}]");
                }
            }
        }
    }
}
