using Acb.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Consul
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Consul作为服务注册和发现的组件 </summary>
        /// <param name="builder"></param>
        /// <param name="server"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IMicroBuilder AddConsul(this IMicroBuilder builder, string server,
            string token = null)
        {
            CacheManager.SetProvider(CacheLevel.First, new RuntimeMemoryCacheProvider());
            builder.Services.AddSingleton<IServiceRegister>(provider => new ConsulServiceRegister(server, token));
            builder.Services.AddSingleton<IServiceFinder>(provider => new ConsulServiceFinder(server, token));
            return builder;
        }
    }
}
