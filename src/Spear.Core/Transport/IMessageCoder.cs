using Spear.Core.Message;

namespace Spear.Core.Transport
{
    /// <summary> 消息编码器 </summary>
    public interface IMessageEncoder
    {
        /// <summary> 消息编码 </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Encode(TransportMessage message);
    }

    /// <summary> 消息解码器 </summary>
    public interface IMessageDecoder
    {
        /// <summary> 消息解码 </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        TransportMessage Decode(byte[] data);
    }

    public interface IMessageCoderFactory
    {
        /// <summary> 获取编码器 </summary>
        /// <returns></returns>
        IMessageEncoder GetEncoder();
        /// <summary> 获取解码器 </summary>
        /// <returns></returns>
        IMessageDecoder GetDecoder();
    }
}
