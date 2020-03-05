using System;
using System.Threading.Tasks;
using Spear.Core;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.WebSocket
{
    [Protocol(ServiceProtocol.Ws)]
    public class WebSocketClientFactory : IMicroClientFactory
    {
        public Task<IMicroClient> CreateClient(ServiceAddress serviceAddress)
        {
            throw new NotImplementedException();
        }
    }
}
