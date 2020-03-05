using System;

namespace Spear.Core.Message
{
    /// <summary> 消息序列化 </summary>
    public interface IMessageSerializer
    {
        byte[] Serialize(object value);

        byte[] SerializeNoType(object value);

        object Deserialize(byte[] data, Type type);

        object DeserializeNoType(byte[] data, Type type);

        T Deserialize<T>(byte[] data);
    }
}
