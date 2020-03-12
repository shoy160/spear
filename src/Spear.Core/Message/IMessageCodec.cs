using System;
using System.Threading.Tasks;

namespace Spear.Core.Message
{
    /// <summary> 消息编码器 </summary>
    public interface IMessageEncoder
    {
        /// <summary> 消息编码 </summary>
        /// <param name="message"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        Task<byte[]> EncodeAsync(object message, bool gzip = true);
    }

    /// <summary> 消息解码器 </summary>
    public interface IMessageDecoder
    {
        /// <summary> 消息解码 </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        Task<object> DecodeAsync(byte[] data, Type type, bool gzip = true);
    }


    public interface IMessageCodec : IClientMessageCodec { }

    public interface IClientMessageCodec : IMessageEncoder, IMessageDecoder { }


    public static class MessageCodecExtensions
    {
        /// <summary> 解码 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static async Task<T> DecodeAsync<T>(this IMessageDecoder decoder, byte[] data, bool gzip = true)
        {
            var obj = await decoder.DecodeAsync(data, typeof(T), gzip);
            return obj.CastTo<T>();
        }

        /// <summary> 编码 </summary>
        /// <param name="encoder"></param>
        /// <param name="message"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static byte[] Encode(this IMessageEncoder encoder, object message, bool gzip = true)
        {
            return encoder.EncodeAsync(message, gzip).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary> 解码 </summary>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static object Decode(this IMessageDecoder decoder, byte[] data, Type type, bool gzip = true)
        {
            return decoder.DecodeAsync(data, type, gzip).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary> 解码 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decoder"></param>
        /// <param name="data"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static T Decode<T>(this IMessageDecoder decoder, byte[] data, bool gzip = true)
        {
            var obj = decoder.Decode(data, typeof(T), gzip);
            return obj.CastTo<T>();
        }
    }
}
