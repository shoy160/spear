using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using Spear.Nacos.Sdk;
using WebApiClient;

namespace Spear.Nacos
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Nacos服务发现 </summary>
        /// <param name="builder"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddNacos(this IMicroClientBuilder builder,
            Action<NacosConfig> configAction = null)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddNacosCore(configAction);
            builder.Services.AddSingleton<IServiceFinder>(provider =>
            {
                var config = provider.GetService<NacosConfig>();
                var client = provider.GetService<INacosClient>();
                var cache = provider.GetService<IMemoryCache>();
                var logger = provider.GetService<ILogger<NacosServiceFinder>>();
                return new NacosServiceFinder(config, client, logger, cache);
            });
            return builder;
        }

        /// <summary> 使用Nacos服务注册 </summary>
        /// <param name="builder"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddNacos(this IMicroServerBuilder builder,
            Action<NacosConfig> configAction = null)
        {
            builder.Services.AddNacosCore(configAction);
            builder.Services.AddSingleton<IServiceRegister>(provider =>
            {
                var config = provider.GetService<NacosConfig>();
                var client = provider.GetService<INacosClient>();
                var helper = provider.GetService<NacosListenerHelper>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new NacosServiceRegister(config, client, helper, loggerFactory);
            });
            return builder;
        }

        /// <summary> 添加Nacos核心 </summary>
        /// <param name="services"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddNacosCore(this IServiceCollection services, Action<NacosConfig> configAction = null)
        {
            var config = NacosConfig.Config();
            configAction?.Invoke(config);
            services.AddSingleton(config);
            services.TryAddSingleton(provider =>
            {
                return HttpApi.Register<INacosClient>().ConfigureHttpApiConfig(c =>
                {
                    c.HttpHost = new Uri(config.Host);
                    c.ServiceProvider = provider;
                });
            });

            services.AddTransient(provider =>
            {
                var factory = provider.GetService<HttpApiFactory<INacosClient>>();
                return factory.CreateHttpApi();
            });
            services.AddSingleton(provider =>
            {
                var client = provider.GetService<INacosClient>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new NacosListenerHelper(client, loggerFactory);
            });
            return services;
        }

        /// <summary> 使用Nacos配置中心 </summary>
        /// <param name="provider"></param>
        public static void AddNacosConfig(this IServiceProvider provider)
        {
            var nacosConfig = provider.GetService<NacosConfig>();
            if (string.IsNullOrWhiteSpace(nacosConfig?.Applications))
                return;
            var client = provider.GetService<INacosClient>();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var listener = provider.GetService<NacosListenerHelper>();

            var builder = provider.GetService<ISpearConfigBuilder>();
            //var manager = ConfigManager.Instance;
            var apps = nacosConfig.Applications.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            var providers = apps
                .Select(app => new NacosConfigProvider(nacosConfig, client, listener, loggerFactory, app)).ToList();
            foreach (var configProvider in providers)
            {
                var config = builder.Sources.FirstOrDefault(t => t is NacosConfigProvider nacos && nacos.App == configProvider.App);
                if (config != null)
                    builder.Sources.Remove(config);

                builder.Sources.Insert(0, configProvider);
            }
        }
    }
}
