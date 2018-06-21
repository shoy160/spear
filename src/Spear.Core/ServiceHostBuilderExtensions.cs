using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Proxy;
using Spear.Core.ServiceHosting;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Core
{
    public static class ServiceHostBuilderExtensions
    {
        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceHostBuilder UseCoder<T>(this IServiceHostBuilder builder) where T : class, IMessageCoderFactory
        {
            builder.Services.AddSingleton<IMessageCoderFactory, T>();
            return builder;
        }

        /// <summary>
        /// 使用编解码器。
        /// </summary>
        /// <typeparam name="T">编解码器工厂实现类型。</typeparam>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceHostBuilder UseJsonCoder(this IServiceHostBuilder builder)
        {
            builder.Services.AddSingleton<IMessageCoderFactory, JsonMessageCoderFactory>();
            return builder;
        }

        public static IServiceHostBuilder UseServer(this IServiceHostBuilder builder, string ip, int port)
        {
            return builder.UseServer(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public static IServiceHostBuilder UseServer(this IServiceHostBuilder builder, EndPoint endPoint)
        {
            builder.Services.AddSingleton<IMicroEntryFactory, MicroEntryFactory>();
            builder.Services.AddSingleton<IMicroExecutor, MicroExecutor>();
            builder.MapServices(provider =>
            {
                var host = provider.GetService<IMicroHost>();
                Task.Factory.StartNew(async () => await host.Start(endPoint))
                    .Wait();
            });

            return builder;
        }

        public static IServiceHostBuilder UseClient(this IServiceHostBuilder builder)
        {
            builder.Services.AddSingleton<IClientProxy, ClientProxy>();
            return builder;
        }
    }
}
