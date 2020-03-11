using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Udp
{
    [Protocol(ServiceProtocol.Udp)]
    public class UdpMicroListener : MicroListener
    {
        private readonly ILogger<UdpMicroListener> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageCodecFactory _codecFactory;

        public UdpMicroListener(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory)
        {
            _loggerFactory = loggerFactory;
            _codecFactory = codecFactory;
            _logger = loggerFactory.CreateLogger<UdpMicroListener>();
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
