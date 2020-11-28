using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Micro;

namespace Spear.Protocol.Http
{
    public static class ServiceCollectionExtensions
    {

        public static bool IsGzip(this HttpResponseMessage resp)
        {
            return resp.Content.Headers.ContentEncoding.Contains("gzip");
        }

        public static bool IsGzip(this HttpContext context)
        {
            return context.Response != null &&
                   context.Response.Headers.TryGetValue("Content-Encoding", out var encoding) &&
                   encoding.Contains("gzip");
        }

        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddHttpProtocol(this IMicroClientBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IMicroClientFactory, HttpClientFactory>();
            return builder;
        }

        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddHttpProtocol(this IMicroServerBuilder builder)
        {
            Constants.Protocol = ServiceProtocol.Http;
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.AddSession<HttpPrincipalAccessor>();
            builder.Services.AddSingleton<IMicroListener, HttpMicroListener>();
            return builder;
        }
    }
}
