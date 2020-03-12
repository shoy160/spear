using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Consul
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 使用Consul作为服务注册和发现的组件
        /// 读取配置：consul
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddConsul(this IMicroClientBuilder builder, Action<ConsulOption> optionAction = null)
        {
            builder.AddMemoryCache();
            builder.AddSingleton<IServiceFinder>(provider =>
            {
                var option = ConsulOption.Config();
                optionAction?.Invoke(option);
                var cache = provider.GetService<IMemoryCache>();
                var logger = provider.GetService<ILogger<ConsulServiceFinder>>();
                return new ConsulServiceFinder(cache, logger, option.Server, option.Token);
            });
            return builder;
        }

        /// <summary>
        /// 使用Consul作为服务注册和发现的组件
        /// 读取配置：consul
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddConsul(this IMicroServerBuilder builder, Action<ConsulOption> optionAction = null)
        {
            builder.AddSingleton<IServiceRegister>(provider =>
            {
                var option = ConsulOption.Config();
                optionAction?.Invoke(option);
                var logger = provider.GetService<ILogger<ConsulServiceRegister>>();
                return new ConsulServiceRegister(logger, option.Server, option.Token);
            });
            return builder;
        }

        /// <summary> 使用Consul作为服务注册和发现的组件 </summary>
        /// <param name="builder"></param>
        /// <param name="server"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddConsul(this IMicroClientBuilder builder, string server,
            string token = null)
        {
            builder.AddMemoryCache();
            builder.AddSingleton<IServiceFinder>(provider =>
            {
                var cache = provider.GetService<IMemoryCache>();
                var logger = provider.GetService<ILogger<ConsulServiceFinder>>();
                return new ConsulServiceFinder(cache, logger, server, token);
            });
            return builder;
        }

        /// <summary> 使用Consul作为服务注册和发现的组件 </summary>
        /// <param name="builder"></param>
        /// <param name="server"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddConsul(this IMicroServerBuilder builder, string server,
            string token = null)
        {
            builder.AddSingleton<IServiceRegister>(provider =>
            {
                var logger = provider.GetService<ILogger<ConsulServiceRegister>>();
                return new ConsulServiceRegister(logger, server, token);
            });
            return builder;
        }

        /// <summary> 使用Consul配置中心 </summary>
        /// <param name="manager"></param>
        public static void UseConsul(this ConfigManager manager)
        {
            //配置中心
            var provider = new ConsulConfigProvider();
            manager.Build(b => b.Add(provider));
            manager.ConfigChanged += provider.Reload;
        }
    }
}
