using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Config;
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
            Constants.Protocol = ServiceProtocol.Tcp;
            builder.Services.AddSingleton<IMicroListener, DotNettyMicroListener>();
            return builder;
        }

        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddTcpProtocol(this IMicroClientBuilder builder)
        {
            builder.Services.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            return builder;
        }
    }
}
