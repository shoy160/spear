using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Codec.ProtoBuffer
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroServerBuilder AddProtoBufCodec(this IMicroServerBuilder builder)
        {
            Constants.Codec = ServiceCodec.ProtoBuf;
            builder.Services.AddSingleton<IMessageSerializer, ProtoBufferSerializer>();
            builder.Services.AddSingleton<IMessageCodec, ProtoBufferCodec>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.ProtoBuf);
                var config = provider.GetService<SpearConfig>();
                return new ProtoBufferCodec(serializer, config);
            });
            return builder;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroClientBuilder AddProtoBufCodec(this IMicroClientBuilder builder)
        {
            builder.Services.AddSingleton<IMessageSerializer, ProtoBufferSerializer>();
            builder.Services.AddSingleton<IClientMessageCodec, ProtoBufferCodec>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.ProtoBuf);
                var config = provider.GetService<SpearConfig>();
                return new ProtoBufferCodec(serializer, config);
            });
            return builder;
        }
    }
}
