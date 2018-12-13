using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.DotNetty
{
    public static class DotNettyExtensions
    {
        /// <summary> 使用DotNetty数据传输组件 </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDotNetty(this IServiceCollection services)
        {
            services.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            services.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                return new DotNettyMicroListener(coderFactory);
            });
            return services;
        }
    }
}
