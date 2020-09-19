using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spear.Core.Config
{
    public interface ISpearConfigBuilder : IConfigurationBuilder
    {
    }

    public static class ConfigBuilderExtensions
    {
        public static IServiceCollection AddSpearConfig(this IServiceCollection services, Action<ISpearConfigBuilder> configBuilderAction = null)
        {
            var configBuilder = new SpearConfigBuilder();
            configBuilder.AddLocal();
            configBuilderAction?.Invoke(configBuilder);
            var config = configBuilder.Build();
            services.AddSingleton(config);
            return services;
        }

        public static ISpearConfigBuilder AddSpearConfig(this IConfigurationBuilder builder, IConfiguration configuration = null)
        {
            var config = configuration ?? builder.Build();
            var acbBuilder = new SpearConfigBuilder(builder, config);
            return acbBuilder.AddLocal();
        }

        /// <summary> 添加本地Json配置 </summary>
        /// <param name="builder"></param>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static ISpearConfigBuilder AddLocal(this ISpearConfigBuilder builder, string dirPath = null)
        {
            var configPath = string.IsNullOrWhiteSpace(dirPath) ? "configPath".Config<string>() : dirPath;
            if (string.IsNullOrWhiteSpace(configPath))
                return builder;
            configPath = Path.Combine(Directory.GetCurrentDirectory(), configPath);
            if (!Directory.Exists(configPath))
                return builder;
            CurrentIocManager.CreateLogger(typeof(ConfigBuilderExtensions)).LogInformation($"正在加载本地配置[{configPath}]");
            var jsons = Directory.GetFiles(configPath, "*.json");
            if (jsons.Any())
            {
                foreach (var json in jsons)
                {
                    builder.AddJsonFile(json, false, true);
                }
            }

            return builder;
        }
    }

    public class SpearConfigBuilder : ISpearConfigBuilder
    {
        private readonly IConfigurationBuilder _builder;
        private readonly ConfigHelper _configHelper;

        public SpearConfigBuilder(IConfigurationBuilder builder = null, IConfiguration config = null)
        {
            if (builder == null)
            {
                builder = ConfigHelper.CreateDefaultBuilder();
            }
            if (config == null)
            {
                config = builder.Build();
            }
            _builder = builder;
            _configHelper = ConfigHelper.Instance;
            _configHelper.Builder = this;
            _configHelper.SetConfig((IConfigurationRoot)config);
        }

        public IConfigurationBuilder Add(IConfigurationSource source)
        {
            _builder.Add(source);
            return this;
        }

        public IConfigurationRoot Build()
        {
            var config = _builder.Build();
            _configHelper.SetConfig(config);
            return config;
        }

        public IDictionary<string, object> Properties => _builder.Properties;
        public IList<IConfigurationSource> Sources => _builder.Sources;
    }
}
