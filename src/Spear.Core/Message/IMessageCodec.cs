using System;
using System.Threading.Tasks;

namespace Spear.Core.Message
{
    /// <summary> 消息编码器 </summary>
    public interface IMessageEncoder
    {
        /// <summary> 消息编码 </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<byte[]> EncodeAsync(object message);
    }

    /// <summary> 消息解码器 </summary>
    public interface IMessageDecoder
    {
        /// <summary> 消息解码 </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<object> DecodeAsync(byte[] data, Type type);
    }

    public interface IMessageCodec : IMessageEncoder, IMessageDecoder { }

    public interface IMessageCodecFactory
    {
        /// <summary> 获取编码器 </summary>
        /// <returns></returns>
        IMessageEncoder GetEncoder();
        /// <summary> 获取解码器 </summary>
        /// <returns></returns>
        IMessageDecoder GetDecoder();
    }

    public static class MessageCodecExtensions
    {
        /// <summary> 解码 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<T> DecodeAsync<T>(this IMessageDecoder decoder, byte[] data)
        {
            var obj = await decoder.DecodeAsync(data, typeof(T));
            return obj.CastTo<T>();
        }

        /// <summary> 编码 </summary>
        /// <param name="encoder"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Encode(this IMessageEncoder encoder, object message)
        {
            return encoder.EncodeAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary> 解码 </summary>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Decode(this IMessageDecoder decoder, byte[] data, Type type)
        {
            return decoder.DecodeAsync(data, type).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary> 解码 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Decode<T>(this IMessageDecoder decoder, byte[] data)
        {
            var obj = decoder.Decode(data, typeof(T));
            return obj.CastTo<T>();
        }
    }
}
