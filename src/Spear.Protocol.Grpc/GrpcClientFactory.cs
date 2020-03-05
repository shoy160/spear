using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Spear.Core;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Grpc
{
    [Protocol(ServiceProtocol.Grpc)]
    public class GrpcClientFactory : IMicroClientFactory
    {
        public Task<IMicroClient> CreateClient(ServiceAddress serviceAddress)
        {
            throw new NotImplementedException();
        }
    }
}
