using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Micro;

namespace Spear.Codec.MessagePack
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroServerBuilder AddMessagePackCodec(this IMicroServerBuilder builder)
        {
            Constants.Codec = ServiceCodec.MessagePack;
            builder.Services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
            builder.Services.AddSingleton<IMessageCodec, MessagePackCodec>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.MessagePack);
                var config = provider.GetService<SpearConfig>();
                return new MessagePackCodec(serializer, config);
            });

            return builder;
        }

        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IMicroClientBuilder AddMessagePackCodec(this IMicroClientBuilder builder)
        {
            builder.Services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
            builder.Services.AddSingleton<IClientMessageCodec, MessagePackCodec>(provider =>
            {
                var serializer = provider.GetService<IMessageSerializer>(ServiceCodec.MessagePack);
                var config = provider.GetService<SpearConfig>();
                return new MessagePackCodec(serializer, config);
            });

            return builder;
        }
    }
}
