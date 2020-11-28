using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Micro;

namespace Spear.Protocol.WebSocket
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用WebSocket传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddWebSocketProtocol(this IMicroServerBuilder builder)
        {
            Constants.Protocol = ServiceProtocol.Ws;
            builder.Services.AddSingleton<IMicroListener, WebSocketListener>();
            return builder;
        }

        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddWebSocketProtocol(this IMicroClientBuilder builder)
        {
            builder.Services.AddSingleton<IMicroClientFactory, WebSocketClientFactory>();
            return builder;
        }
    }
}
