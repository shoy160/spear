using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Tcp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddTcpProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new DotNettyMicroListener(loggerFactory, coderFactory);
            });
            return builder;
        }

        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddTcpProtocol(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            return builder;
        }
    }
}
