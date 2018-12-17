using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.DotNetty
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty客户端 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder AddDotNettyClient(this IMicroBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            return builder;
        }

        /// <summary> 使用DotNetty的TCP传输协议 </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IMicroBuilder AddDotNettyTcp(this IMicroBuilder services)
        {
            services.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                return new DotNettyMicroListener(coderFactory);
            });
            return services;
        }
    }
}
