using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Http传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder AddHttpProtocol(this IMicroBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, HttpClientFactory>();

            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                return new HttpMicroListener(coderFactory);
            });
            return builder;
        }
    }
}
