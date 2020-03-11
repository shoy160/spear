using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Grpc
{
    [Protocol(ServiceProtocol.Grpc)]
    public class GrpcMicroListener : MicroListener
    {
        private Server _grpcServer;

        private readonly ILogger<GrpcMicroListener> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly IServiceProvider _provider;

        public GrpcMicroListener(ILoggerFactory loggerFactory, IMicroEntryFactory entryFactory, IServiceProvider provider)
        {
            _loggerFactory = loggerFactory;
            _entryFactory = entryFactory;
            _provider = provider;
            _logger = loggerFactory.CreateLogger<GrpcMicroListener>();
        }

        public override Task Start(ServiceAddress serviceAddress)
        {
            _grpcServer = new Server
            {
                Ports =
                {
                    new ServerPort(serviceAddress.Service, serviceAddress.Port, ServerCredentials.Insecure)
                }
            };
            var services = _entryFactory.Services;
            foreach (var type in services)
            {
                var baseType = type?.BaseType;
                var definitionType = baseType?.DeclaringType;
                var methodInfo = definitionType?.GetMethod("BindService", new[] { baseType });
                if (methodInfo != null)
                {
                    var instance = _provider.GetService(type);
                    var service = methodInfo.Invoke(null, new[] { instance });
                    if (service is ServerServiceDefinition serviceDescriptor)
                    {
                        _grpcServer.Services.Add(serviceDescriptor);
                    }
                }
            }

            _logger.LogInformation($"发现 GRPC服务：{_grpcServer.Services.Count()} 条");
            _logger.LogInformation($"GRPC Service Start At:{serviceAddress}");
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _grpcServer.Start();
            return Task.CompletedTask;
        }

        public override async Task Stop()
        {
            if (_grpcServer != null)
                await _grpcServer.ShutdownAsync();
        }
    }
}
