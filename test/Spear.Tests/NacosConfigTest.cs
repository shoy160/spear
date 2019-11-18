using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spear.Core;
using Spear.Nacos;
using Spear.Nacos.Sdk;
using Spear.Nacos.Sdk.Requests;
using System;
using System.Threading.Tasks;
using Spear.Nacos.Sdk.Requests.Config;

namespace Spear.Tests
{
    [TestClass]
    public class NacosConfigTest : BaseTest
    {
        private readonly INacosClient _client;
        public NacosConfigTest()
        {
            _client = Resolve<INacosClient>();
        }

        protected override void MapServices(IServiceCollection services)
        {
            services.AddNacosCore(config =>
            {
                config.Host = "http://192.168.0.231:8848/";
                config.Tenant = "ef950bae-865b-409b-9c3b-bc113cf7bf37";
                config.Applications = "oss";
                config.Interval = 0;
            });
            base.MapServices(services);
        }

        protected override void UseServices(IServiceProvider provider)
        {
            provider.UseNacosConfig();
            base.UseServices(provider);
        }

        [TestMethod]
        public async Task GetTest()
        {
            var request = new GetConfigRequest
            {
                DataId = "redis"
            };
            var config = await _client.GetConfigAsync(request);
            Print(config);
        }

        [TestMethod]
        public void ConfigTest()
        {
            var config = "oss:keyId".Config<string>();
            Print(config);
        }

        [TestMethod]
        public async Task PublishTest()
        {
            var request = new PublishConfigRequest
            {
                DataId = "test",
                Content = "{\"shay\":123456}",
                Type = "json"
            };
            var result = await _client.PublishConfigAsync(request);
            Print(result);
        }

        [TestMethod]
        public async Task RemoveTest()
        {
            var request = new RemoveConfigRequest
            {
                DataId = "test"
            };
            var result = await _client.RemoveConfigAsync(request);
            Print(result);
        }
    }
}
