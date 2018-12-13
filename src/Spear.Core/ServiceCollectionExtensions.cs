using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Core.Proxy;
using System;
using System.Threading.Tasks;

namespace Spear.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <param name="services">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceCollection AddCoder<T>(this IServiceCollection services) where T : class, IMessageCoderFactory
        {
            services.AddSingleton<IMessageCoderFactory, T>();
            return services;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="services">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceCollection AddJsonCoder(this IServiceCollection services)
        {
            services.AddCoder<JsonMessageCoderFactory>();
            return services;
        }

        /// <summary> 添加微服务 </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServer(this IServiceCollection services)
        {
            services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
            services.AddSingleton<IMicroExecutor, MicroExecutor>();
            services.AddSingleton<IMicroHost, MicroHost>();
            return services;
        }

        /// <summary> 添加微服务客户端 </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClient(this IServiceCollection services)
        {
            services.AddSingleton<IClientProxy, ClientProxy>();
            return services;
        }

        /// <summary> 使用微服务 </summary>
        /// <param name="provider"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IServiceProvider UseServer(this IServiceProvider provider, string host, int port)
        {
            return provider.UseServer(address =>
            {
                address.Host = host;
                address.Port = port;
            });
        }

        /// <summary> 使用微服务 </summary>
        /// <param name="provider"></param>
        /// <param name="addressAction"></param>
        /// <returns></returns>
        public static IServiceProvider UseServer(this IServiceProvider provider, Action<ServiceAddress> addressAction)
        {
            var address = new ServiceAddress();
            addressAction?.Invoke(address);
            var host = provider.GetService<IMicroHost>();
            Task.Factory.StartNew(async () => await host.Start(address));
            return provider;
        }

        public static IServiceProvider UseClient(this ServiceProvider provider)
        {
            ClientContext.Provider = provider;
            return provider;
        }
    }
}
