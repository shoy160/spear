using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Tcp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder AddTcpProtocol(this IMicroBuilder builder)
        {
            Constants.Protocol = ServiceProtocol.Http;

            builder.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                var logger = provider.GetService<ILogger<DotNettyMicroListener>>();
                return new DotNettyMicroListener(logger, coderFactory);
            });
            return builder;
        }
    }
}
