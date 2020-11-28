using System;
using System.Linq;
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
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IServiceFinder>(provider =>
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
            builder.Services.AddSingleton<IServiceRegister>(provider =>
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
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IServiceFinder>(provider =>
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
            builder.Services.AddSingleton<IServiceRegister>(provider =>
            {
                var logger = provider.GetService<ILogger<ConsulServiceRegister>>();
                return new ConsulServiceRegister(logger, server, token);
            });
            return builder;
        }

        /// <summary> 使用Consul配置中心 </summary>
        /// <param name="manager"></param>
        public static ISpearConfigBuilder AddConsulConfig(this ISpearConfigBuilder builder)
        {
            //配置中心
            var provider = new ConsulConfigProvider();
            var t = builder.Sources.FirstOrDefault(c => c is ConsulConfigProvider);
            if (t != null)
                builder.Sources.Remove(t);
            builder.Sources.Insert(0, provider);
            return builder;
        }
    }
}
