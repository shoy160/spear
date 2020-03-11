using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;

namespace Spear.Codec.ProtoBuffer
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddProtoBufCodec<T>(this T builder) where T : IMicroBuilder
        {
            Constants.Codec = ServiceCodec.ProtoBuf;
            builder.AddSingleton<IMessageSerializer, ProtoBufferSerializer>();
            builder.AddSingleton<ProtoBufferCodec>();
            builder.TryAddScoped<IMessageCodecFactory, DMessageCodecFactory<ProtoBufferCodec>>();
            //builder.TryAddSingleton<IMessageCodecFactory>(provider =>
            //{
            //    var serializer = provider.GetService<IMessageSerializer>();
            //    var codec = new ProtoBufferCodec(serializer);
            //    return new DMessageCodecFactory<ProtoBufferCodec>(codec);
            //});
            return builder;
        }
    }
}
