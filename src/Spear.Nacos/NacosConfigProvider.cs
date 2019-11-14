using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spear.Core.Config;
using Spear.Nacos.Sdk;
using Spear.Nacos.Sdk.Requests;
using System;
using System.Threading.Tasks;

namespace Spear.Nacos
{
    public class NacosConfigProvider : DConfigProvider, IConfigurationSource
    {
        private readonly NacosConfig _config;
        private readonly INacosClient _client;
        private readonly NacosListenerHelper _listenerHelper;
        private readonly ILogger<NacosConfigProvider> _logger;
        private readonly string _application;

        public NacosConfigProvider(NacosConfig config, INacosClient client, NacosListenerHelper listenerHelper, ILoggerFactory loggerFactory,
             string application)
        {
            _config = config;
            _client = client;
            _application = application;
            _listenerHelper = listenerHelper;
            _logger = loggerFactory.CreateLogger<NacosConfigProvider>();
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(_application) || _config.Interval <= 0)
                return this;
            _listenerHelper.Add(new AddListenerRequest
            {
                DataId = _application,
                Group = _config.Group,
                Tenant = _config.Tenant
            }, async newConfig =>
            {
                _logger.LogInformation($"配置更新：{_application}");
                await LoadConfig();
            }, _config.Interval, _config.LongPollingTimeout);

            return this;
        }

        private async Task LoadConfig()
        {
            try
            {
                var request = new GetConfigRequest
                {
                    DataId = _application
                };
                if (!string.IsNullOrWhiteSpace(_config.Group))
                    request.Group = _config.Group;
                if (!string.IsNullOrWhiteSpace(_config.Tenant))
                    request.Tenant = _config.Tenant;
                var config = await _client.GetConfigAsync(request);

                _listenerHelper.UpdateCache(request, config);
                Data.Clear();
                if (!string.IsNullOrWhiteSpace(config))
                    LoadJson(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"加载配置[{_application}]异常:{ex.Message}");
            }
        }

        public override void Load()
        {
            LoadConfig().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override void Reload(object state = null)
        {
        }
    }
}
