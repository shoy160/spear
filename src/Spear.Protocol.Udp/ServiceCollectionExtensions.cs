using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Udp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的UDP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddUdpProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new UdpMicroListener(loggerFactory, coderFactory);
            });
            return builder;
        }

        /// <summary> 使用DotNetty的UDP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddUdpProtocol(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, UdpClientFactory>();
            return builder;
        }
    }
}
