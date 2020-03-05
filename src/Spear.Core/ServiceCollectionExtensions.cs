using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spear.Core.Message;
using Spear.Core.Message.Codec;
using Spear.Core.Message.Implementation;
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
        private static readonly IDictionary<MethodInfo, string> RouteCache = new Dictionary<MethodInfo, string>();
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

        /// <summary> 获取服务主键 </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string ServiceKey(this MethodInfo method)
        {
            if (RouteCache.TryGetValue(method, out var route))
                return route;
            var key = string.Empty;
            var attr = method.DeclaringType?.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null)
                key = attr.Route;
            attr = method.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Route))
                route = (attr.Route.StartsWith("/") ? attr.Route.TrimStart('/') : $"{key}/{attr.Route}").ToLower();
            else if (!string.IsNullOrWhiteSpace(key))
            {
                route = $"{key}/{method.Name}".ToLower();
            }
            else
            {
                route = $"{method.DeclaringType?.Name}/{method.Name}".ToLower();
            }
            RouteCache.Add(method, route);

            return route;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddJsonCodec<T>(this T builder) where T : IMicroBuilder
        {
            builder.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            builder.TryAddSingleton<IMessageCodecFactory>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>();
                var codec = new JsonCodec(serializer);
                return new DMessageCodecFactory<JsonCodec>(codec);
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
        /// <returns></returns>
        public static IServiceCollection AddMicroClient(this IMicroClientBuilder services, Action<IMicroClientBuilder> builderAction)
        {
            builderAction.Invoke(services);
            //services.TryAddSingleton<Counter>();
            services.AddProxy<ClientProxy>();
            return services;
        }

        /// <summary> 添加微服务 </summary>
        /// <param name="services"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroService(this IMicroServerBuilder services, Action<IMicroServerBuilder> builderAction)
        {
            builderAction.Invoke(services);
            services.AddSingleton<IAssemblyFinder, DefaultAssemblyFinder>();
            services.AddSingleton<ITypeFinder, DefaultTypeFinder>();
            services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
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
