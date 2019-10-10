using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Core.Proxy;
using Spear.Core.Reflection;
using Spear.ProxyGenerator;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core
{
    public static class ServiceCollectionExtensions
    {
        public static T GetService<T>(this IServiceProvider provider, ServiceProtocol protocol)
        {
            var list = provider.GetServices<T>();
            return list.First(t => t.GetType().GetCustomAttribute<ProtocolAttribute>()?.Protocol == protocol);
        }

        public static object GetService(this IServiceProvider provider, Type type, ServiceProtocol protocol)
        {
            var list = provider.GetServices(type);
            return list.First(t => t.GetType().GetCustomAttribute<ProtocolAttribute>()?.Protocol == protocol);
        }

        public static string ServiceKey(this MethodInfo method)
        {
            var key = string.Empty;
            var attr = method.DeclaringType?.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null)
                key = attr.Route;
            attr = method.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Route))
                return (attr.Route.StartsWith("/") ? attr.Route.TrimStart('/') : $"{key}/{attr.Route}").ToLower();
            if (!string.IsNullOrWhiteSpace(key))
            {
                return $"{key}/{method.Name}".ToLower();
            }

            return $"{method.DeclaringType?.Name}/{method.Name}".ToLower();
        }

        /// <summary> 使用编解码器。 </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <typeparam name="TCodec"></typeparam>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddCoder<T, TCodec>(this T builder)
            where T : IMicroBuilder
            where TCodec : class, IMessageCodecFactory
        {
            builder.TryAddSingleton<IMessageCodecFactory, TCodec>();
            return builder;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddJsonCoder<T>(this T builder) where T : IMicroBuilder
        {
            builder.AddCoder<T, JsonMessageCodecFactory>();
            return builder;
        }

        /// <summary> 添加微服务客户端 </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroClient(this IServiceCollection services, Action<IMicroClientBuilder> builderAction)
        {
            var builder = new MicroBuilder(services);
            builderAction.Invoke(builder);
            services.AddProxy<ClientProxy>();
            return services;
        }

        /// <summary> 添加微服务 </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroService(this IServiceCollection services, Action<IMicroServerBuilder> builderAction)
        {
            var builder = new MicroBuilder(services);
            builderAction.Invoke(builder);
            services.AddSingleton<IAssemblyFinder, DefaultAssemblyFinder>();
            services.AddSingleton<ITypeFinder, DefaultTypeFinder>();
            services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
            services.AddSingleton<IMicroExecutor, MicroExecutor>();
            services.AddSingleton<IMicroHost, MicroHost>();
            return services;
        }

        /// <summary> 开启微服务 </summary>
        /// <param name="provider"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static void UseMicroService(this IServiceProvider provider, string host, int port)
        {
            provider.UseMicroService(address =>
            {
                address.Host = host;
                address.Port = port;
            });
        }

        /// <summary> 开启微服务 </summary>
        /// <param name="provider"></param>
        /// <param name="addressAction"></param>
        /// <returns></returns>
        public static void UseMicroService(this IServiceProvider provider, Action<ServiceAddress> addressAction)
        {
            var address = new ServiceAddress();
            addressAction?.Invoke(address);
            var host = provider.GetService<IMicroHost>();
            Task.Factory.StartNew(async () => await host.Start(address));
        }
    }
}
