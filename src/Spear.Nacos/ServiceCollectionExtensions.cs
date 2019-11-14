using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Nacos.Sdk;
using System;
using WebApiClient;

namespace Spear.Nacos
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacos(this IServiceCollection services, Action<NacosConfig> configAction = null)
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

            //services.AddSingleton(provider =>
            //{
            //    var client = provider.GetService<INacosClient>();
            //    var loggerFactory = provider.GetService<ILoggerFactory>();
            //    var configProvider = new NacosConfigProvider(config, client, loggerFactory);
            //    return configProvider;
            //});
            return services;
        }

        /// <summary> 使用Nacos配置中心 </summary>
        /// <param name="provider"></param>
        public static void UseNacosConfig(this IServiceProvider provider)
        {
            var nacosConfig = provider.GetService<NacosConfig>();
            if (string.IsNullOrWhiteSpace(nacosConfig?.Applications))
                return;
            var client = provider.GetService<INacosClient>();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var listener = provider.GetService<NacosListenerHelper>();
            var manager = ConfigManager.Instance;
            var apps = nacosConfig.Applications.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var app in apps)
            {
                var configProvider = new NacosConfigProvider(nacosConfig, client, listener, loggerFactory, app);
                manager.Build(b => b.Add(configProvider));
                manager.ConfigChanged += configProvider.Reload;
            }
        }
    }
}
