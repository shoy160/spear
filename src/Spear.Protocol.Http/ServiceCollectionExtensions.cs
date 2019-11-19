using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddHttpProtocol(this IMicroClientBuilder builder)
        {
            builder.AddHttpClient();
            builder.AddSingleton<IMicroClientFactory, HttpClientFactory>();
            return builder;
        }

        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddHttpProtocol(this IMicroServerBuilder builder)
        {
            builder.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.AddSession<HttpPrincipalAccessor>();
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                var entryFactory = provider.GetService<IMicroEntryFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new HttpMicroListener(coderFactory, entryFactory, loggerFactory);
            });
            return builder;
        }
    }
}
