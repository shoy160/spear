using Microsoft.Extensions.Logging;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using System;
using System.Threading.Tasks;

namespace Spear.Protocol.Grpc
{
    [Protocol(ServiceProtocol.Grpc)]
    public class GrpcClientFactory : DMicroClientFactory
    {
        public GrpcClientFactory(ILoggerFactory loggerFactory, IServiceProvider provider, IMicroExecutor microExecutor = null)
            : base(loggerFactory, provider, microExecutor)
        {
        }

        protected override Task<IMicroClient> Create(ServiceAddress address)
        {
            return null;
        }
    }
}
