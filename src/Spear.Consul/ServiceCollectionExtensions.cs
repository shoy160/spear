using Acb.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Micro.Services;

namespace Spear.Consul
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Consul作为服务注册和发现的组件 </summary>
        /// <param name="services"></param>
        /// <param name="server"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsul(this IServiceCollection services, string server,
            string token = null)
        {
            CacheManager.SetProvider(CacheLevel.First, new RuntimeMemoryCacheProvider());
            services.AddSingleton<IServiceRegister>(provider => new ConsulServiceRegister(server, token));
            services.AddSingleton<IServiceFinder>(provider => new ConsulServiceFinder(server, token));
            return services;
        }
    }
}
