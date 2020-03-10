using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Micro.Services;
using Spear.Nacos.Sdk;
using Spear.Nacos.Sdk.Requests.Service;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Spear.Core.Exceptions;

namespace Spear.Nacos
{
    public class NacosServiceRegister : DServiceRegister
    {
        private readonly NacosConfig _config;
        private readonly INacosClient _client;
        private readonly List<RemoveInstanceRequest> _services;
        private readonly ILogger<NacosServiceRegister> _logger;
        private readonly NacosListenerHelper _listenerHelper;

        public NacosServiceRegister(NacosConfig config, INacosClient client, NacosListenerHelper listenerHelper, ILoggerFactory loggerFactory)
        {
            _config = config;
            _client = client;
            _listenerHelper = listenerHelper;
            _logger = loggerFactory.CreateLogger<NacosServiceRegister>();
            _services = new List<RemoveInstanceRequest>();
        }

        public override async Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress)
        {
            foreach (var assembly in assemblyList)
            {
                var serviceName = assembly.ServiceName();
                var meta = new Dictionary<string, string>
                {
                    {KeyService, serverAddress.ToJson()},
                    {KeyMode, $"{Constants.Mode}"},
                    {KeyVersion, assembly.GetName().Version.ToString()}
                };
                var request = new CreateInstanceRequest
                {
                    serviceName = serviceName,
                    namespaceId = _config.Tenant,
                    groupName = _config.Group,
                    clusterName = string.Empty,
                    ip = serverAddress.IpAddress,
                    port = serverAddress.Port,
                    weight = serverAddress.Weight,
                    Meta = meta,
                    enabled = true,
                    healthy = true,
                    ephemeral = false
                };
                try
                {
                    var result = await _client.CreateInstance(request);
                    if (result == "ok")
                    {
                        _services.Add(new RemoveInstanceRequest
                        {
                            namespaceId = request.namespaceId,
                            serviceName = request.serviceName,
                            groupName = request.groupName,
                            clusterName = request.clusterName,
                            ip = request.ip,
                            port = request.port,
                            ephemeral = request.ephemeral
                        });
                        _logger.LogInformation($"服务注册成功 [{serviceName},{serverAddress}]");
                        //发送心跳包
                        _listenerHelper.AddServiceBeat(request, t => { }, 15);
                    }
                    else
                    {
                        throw new SpearException($"注册实例失败,{result}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"服务注册失败 [{serviceName},{serverAddress}]:{ex.Message}");
                }
            }
        }

        public override async Task Deregist()
        {
            foreach (var request in _services)
            {
                await _client.RemoveInstance(request);
            }
            _listenerHelper.Clean();
        }
    }
}
