using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;

namespace Spear.Codec.MessagePack
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddMessagePackCodec<T>(this T builder) where T : IMicroBuilder
        {
            Constants.Codec = ServiceCodec.MessagePack;
            builder.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();

            //builder.AddTransient<IMessageDynamic, MessagePackDynamic>();
            //builder.AddTransient<IMessageInvoke<MessagePackDynamic>>(provider =>
            //{
            //    return new MessagePackInvoke();
            //});
            //builder.AddTransient<IMessageResult<>, MessagePackResult>();
            builder.AddSingleton<MessagePackCodec>();
            builder.TryAddScoped<IMessageCodecFactory, DMessageCodecFactory<MessagePackCodec>>();
            //builder.TryAddSingleton<IMessageCodecFactory>(provider =>
            //{
            //    var serializer = provider.GetService<IMessageSerializer>();
            //    var codec = new MessagePackCodec(serializer);
            //    return new DMessageCodecFactory<MessagePackCodec>(codec);
            //});
            return builder;
        }
    }
}
