using Spear.Core;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;

namespace Spear.Codec
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 使用Json编解码器。 </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static T AddMessagePackCodec<T>(this T builder) where T : IMicroBuilder
        {
            builder.AddCoder<T, DMessageCodecFactory<MessagePackCodec>>();
            return builder;
        }
    }
}
