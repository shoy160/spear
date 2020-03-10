using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spear.Core.Micro;
using Spear.ProxyGenerator;

namespace Spear.Protocol.Grpc
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的GRpc传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddGrpcProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroEntryFactory, GrpcEntryFactory>();
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var entryFactory = provider.GetService<IMicroEntryFactory>();
                return new GrpcMicroListener(loggerFactory, entryFactory, provider);
            });
            return builder;
        }

        /// <summary> 使用DotNetty的GRpc传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddGrpcProtocol(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IProxyFactory, GrpcProxyFactory>();
            builder.AddSingleton<IMicroClientFactory, GrpcClientFactory>();
            return builder;
        }
    }
}
