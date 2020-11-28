using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
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
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Spear.Core.Dependency;

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
            builder.Services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            builder.Services.AddSingleton<IMessageCodec, JsonCodec>(provider =>
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
            builder.Services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            builder.Services.AddSingleton<IClientMessageCodec, JsonCodec>(provider =>
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
            builder.Services.AddScoped<IPrincipalAccessor, T>();
            builder.Services.AddScoped<IMicroSession, ClaimMicroSession>();
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
            builder.Services.AddScoped<IPrincipalAccessor, T>();
            builder.Services.AddScoped<IMicroSession, ClaimMicroSession>();
            return builder;
        }

        /// <summary> 添加默认服务路由 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="routerAction"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddDefaultRouter(this IMicroServerBuilder builder, Action<DefaultServiceRouter> routerAction = null)
        {
            builder.Services.AddSingleton<DefaultServiceRouter>();
            builder.Services.AddSingleton<IServiceRegister>(provider =>
            {
                var router = provider.GetService<DefaultServiceRouter>();
                routerAction?.Invoke(router);
                return router;
            });
            return builder;
        }

        /// <summary> 添加默认服务路由 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="routerAction"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddDefaultRouter(this IMicroClientBuilder builder, Action<DefaultServiceRouter> routerAction = null)
        {
            builder.Services.AddSingleton<DefaultServiceRouter>();
            builder.Services.AddSingleton<IServiceFinder>(provider =>
            {
                var router = provider.GetService<DefaultServiceRouter>();
                routerAction?.Invoke(router);
                return router;
            });
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
            var config = SpearConfig.GetConfig();
            configAction?.Invoke(config);
            services.Services.AddSingleton(config);
            services.Services.AddProxy<ClientProxy>();
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
            services.Services.AddSingleton(config);
            services.Services.AddSingleton<IAssemblyFinder, DefaultAssemblyFinder>();
            services.Services.AddSingleton<ITypeFinder, DefaultTypeFinder>();
            services.Services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
            builderAction.Invoke(services);
            services.Services.AddSingleton<IMicroExecutor, MicroExecutor>();
            services.Services.AddSingleton<IMicroHost, MicroHost>();
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
        public static void UseMicroService(this IServiceProvider provider, Action<ServiceAddress> addressAction = null)
        {
            if (CurrentIocManager.IocManager == null)
                CurrentIocManager.SetIocManager(new DefaultIocManager(provider));

            var address = SpearConfig.GetConfig().Service;
            addressAction?.Invoke(address);
            var host = provider.GetService<IMicroHost>();
            Task.Factory.StartNew(async () => await host.Start(address));
        }
    }
}
