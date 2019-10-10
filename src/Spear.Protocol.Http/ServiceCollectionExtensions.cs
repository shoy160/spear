using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddHttpProtocol(this IMicroClientBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.AddSingleton<IMicroClientFactory, HttpClientFactory>();
            return builder;
        }

        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddHttpProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                return new HttpMicroListener(coderFactory);
            });
            return builder;
        }
    }
}
