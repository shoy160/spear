using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder AddHttpProtocol(this IMicroBuilder builder)
        {
            Constants.Protocol = ServiceProtocol.Http;
            builder.Services.AddHttpClient();
            builder.AddSingleton<IMicroClientFactory, HttpClientFactory>();
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                return new HttpMicroListener(coderFactory);
            });
            return builder;
        }
    }
}
