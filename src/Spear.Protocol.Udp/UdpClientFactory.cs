using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Udp
{
    [Protocol(ServiceProtocol.Udp)]
    public class UdpClientFactory : DMicroClientFactory
    {
        public UdpClientFactory(ILoggerFactory loggerFactory, IServiceProvider provider, IMicroExecutor microExecutor = null)
            : base(loggerFactory, provider, microExecutor)
        {
        }

        protected override Task<IMicroClient> Create(ServiceAddress address)
        {
            throw new NotImplementedException();
        }
    }
}
