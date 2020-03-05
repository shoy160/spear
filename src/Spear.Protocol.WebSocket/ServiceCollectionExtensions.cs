using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.WebSocket
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用WebSocket传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddGrpcProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new WebSocketListener(loggerFactory, coderFactory);
            });
            return builder;
        }

        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddGrpcProtocol(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, WebSocketClientFactory>();
            return builder;
        }
    }
}
