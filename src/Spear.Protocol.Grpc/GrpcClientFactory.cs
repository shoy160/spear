using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Grpc
{
    [Protocol(ServiceProtocol.Grpc)]
    public class GrpcClientFactory : DMicroClientFactory
    {
        public GrpcClientFactory(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory, IMicroExecutor microExecutor = null)
            : base(loggerFactory, codecFactory, microExecutor)
        {
        }

        protected override Task<IMicroClient> Create(ServiceAddress address)
        {
            throw new NotImplementedException();
        }
    }
}
