using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Protocol.Grpc
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用DotNetty的GRpc传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroServerBuilder AddGrpcProtocol(this IMicroServerBuilder builder)
        {
            builder.AddSingleton<IMicroListener>(provider =>
            {
                var coderFactory = provider.GetService<IMessageCodecFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return new GrpcMicroListener(loggerFactory, coderFactory);
            });
            return builder;
        }

        /// <summary> 使用DotNetty的GRpc传输协议 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroClientBuilder AddGrpcProtocol(this IMicroClientBuilder builder)
        {
            builder.AddSingleton<IMicroClientFactory, GrpcClientFactory>();
            return builder;
        }
    }
}
