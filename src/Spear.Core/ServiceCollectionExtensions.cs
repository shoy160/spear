using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Json;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Core.Proxy;
using Spear.Core.Reflection;
using Spear.Core.Session;
using Spear.Core.Session.Impl;
using Spear.ProxyGenerator;

namespace Spear.Core
{
    public static class ServiceCollectionExtensions
    {

        /// <summary> 获取服务 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider provider, ServiceProtocol protocol)
        {
            var list = provider.GetServices<T>();
            return list.First(t => t.GetType().GetCustomAttribute<ProtocolAttribute>()?.Protocol == protocol);
        }

        /// <summary> 获取服务 </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static object GetService(this IServiceProvider provider, Type type, ServiceProtocol protocol)
        {
            var list = provider.GetServices(type);
            return list.First(t => t.GetType().GetCustomAttribute<ProtocolAttribute>()?.Protocol == protocol);
        }

        /// <summary> 获取服务 </summary>
        /// <param name="provider"></param>
        /// <param name="codec"></param>
        /// <returns></returns>
        public static IClientMessageCodec GetClientCodec(this IServiceProvider provider, ServiceCodec codec)
        {
            var messageCodec = provider.GetService<IClientMessageCodec>(codec);
            if (messageCodec == null)
                throw ErrorCodes.ClientError.CodeException($"不支持消息编解码{codec}");
            provider.GetService<ILoggerFactory>().CreateLogger("codec").LogInformation($"使用编解码器：{codec}");
            return messageCodec;
        }

        /// <summary> 获取服务 </summary>
        /// <param name="provider"></param>
        /// <param name="codec"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider provider, ServiceCodec codec)
        {
            var list = provider.GetServices<T>();
            return list.FirstOrDefault(t => t.GetType().GetCustomAttribute<CodecAttribute>()?.Codec == codec);
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroServerBuilder AddJsonCodec(this IMicroServerBuilder builder)
        {
            Constants.Codec = ServiceCodec.Json;
            builder.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            builder.AddSingleton<IMessageCodec, JsonCodec>(provider =>
             {
                 var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.Json);
                 var config = provider.GetService<SpearConfig>();
                 return new JsonCodec(serializer, config);
             });
            return builder;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroClientBuilder AddJsonCodec(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            builder.AddSingleton<IClientMessageCodec, JsonCodec>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.Json);
                var config = provider.GetService<SpearConfig>();
                return new JsonCodec(serializer, config);
            });
            return builder;
        }

        /// <summary> 使用Session。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroClientBuilder AddSession(this IMicroClientBuilder builder)
        {
            //Session
            builder.AddSession<DefaultPrincipalAccessor>();
            return builder;
        }

        /// <summary> 使用Session。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroClientBuilder AddSession<T>(this IMicroClientBuilder builder)
            where T : class, IPrincipalAccessor
        {
            //Session
            builder.AddScoped<IPrincipalAccessor, T>();
            builder.AddScoped<IMicroSession, ClaimMicroSession>();
            return builder;
        }

        /// <summary> 使用Session。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroServerBuilder AddSession(this IMicroServerBuilder builder)
        {
            //Session
            builder.AddSession<DefaultPrincipalAccessor>();
            return builder;
        }

        /// <summary> 使用Session。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroServerBuilder AddSession<T>(this IMicroServerBuilder builder)
            where T : class, IPrincipalAccessor
        {
            //Session
            builder.AddScoped<IPrincipalAccessor, T>();
            builder.AddScoped<IMicroSession, ClaimMicroSession>();
            return builder;
        }

        /// <summary> 添加微服务客户端 </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static MicroBuilder AddMicroClient(this MicroBuilder services, Action<IMicroClientBuilder> builderAction, Action<SpearConfig> configAction = null)
        {
            //services.TryAddSingleton<Counter>();
            var config = "spear".Config<SpearConfig>() ?? new SpearConfig();
            configAction?.Invoke(config);
            services.AddSingleton(config);
            services.AddProxy<ClientProxy>();
            builderAction.Invoke(services);
            return services;
        }

        /// <summary> 添加微服务 </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static MicroBuilder AddMicroService(this MicroBuilder services,
            Action<IMicroServerBuilder> builderAction, Action<SpearConfig> configAction = null)
        {
            var config = "spear".Config<SpearConfig>() ?? new SpearConfig();
            configAction?.Invoke(config);
            services.AddSingleton(config);
            services.AddSingleton<IAssemblyFinder, DefaultAssemblyFinder>();
            services.AddSingleton<ITypeFinder, DefaultTypeFinder>();
            services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
            builderAction.Invoke(services);
            services.AddSingleton<IMicroExecutor, MicroExecutor>();
            services.AddSingleton<IMicroHost, MicroHost>();
            //services.TryAddSingleton<Counter>();
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
