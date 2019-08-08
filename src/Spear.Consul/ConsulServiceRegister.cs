using Consul;
using Microsoft.Extensions.Logging;
using Spear.Core;
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
        private readonly ILogger<ConsulServiceRegister> _logger;

        public ConsulServiceRegister(ILogger<ConsulServiceRegister> logger, string server, string token)
        {
            _consulServer = server;
            _consulToken = token;
            _logger = logger;
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

        /// <summary> 服务注册 </summary>
        /// <param name="assemblyList"></param>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        public async Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress)
        {
            using (var client = CreateClient())
            {
                foreach (var ass in assemblyList)
                {
                    var serviceName = ass.GetName().Name;
                    var service = new AgentServiceRegistration
                    {
                        ID = $"{serviceName}_{serverAddress}".Md5(),
                        Name = serviceName,
                        Tags = new[] { $"{Constants.Mode}" },
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
                        _logger.LogWarning(
                            $"服务注册失败 [{serviceName},{serverAddress}]:{result.StatusCode},{result.RequestTime}");
                    else
                        _logger.LogInformation($"服务注册成功 [{serviceName},{serverAddress}]");
                }
            }
        }

        /// <inheritdoc />
        /// <summary> 服务注销 </summary>
        /// <returns></returns>
        public async Task Deregist()
        {
            using (var client = CreateClient())
            {
                foreach (var service in _services)
                {
                    await client.Agent.ServiceDeregister(service);
                    _logger.LogInformation($"注销服务 [{service}]");
                }
            }
        }
    }
}
