using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Tcp
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder AddTcpProtocol(this IMicroBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();

            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                return new DotNettyMicroListener(coderFactory);
            });
            return builder;
        }
    }
}
