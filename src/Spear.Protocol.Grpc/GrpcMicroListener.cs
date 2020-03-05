using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Grpc
{
    [Protocol(ServiceProtocol.Grpc)]
    public class GrpcMicroListener : MicroListener
    {
        private readonly ILogger<GrpcMicroListener> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageCodecFactory _codecFactory;

        public GrpcMicroListener(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory)
        {
            _loggerFactory = loggerFactory;
            _codecFactory = codecFactory;
            _logger = loggerFactory.CreateLogger<GrpcMicroListener>();
        }

        public override Task Start(ServiceAddress serviceAddress)
        {
            throw new NotImplementedException();
        }

        public override Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
