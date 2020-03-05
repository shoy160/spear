using System;
using MessagePack;

namespace Spear.Codec
{
    public static class MessagePackExtensions
    {
        public static byte[] Serialize(this object value)
        {
            if (value == null) return new byte[0];

            return MessagePackSerializer.Serialize(value);
        }

        public static object Deserialize(this byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.Deserialize(type, data);
        }

        public static T Deserialize<T>(this byte[] data)
        {
            return data == null ? default : MessagePackSerializer.Deserialize<T>(data);
        }
    }
}
