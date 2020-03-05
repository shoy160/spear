using System;
using System.IO;
using ProtoBuf;

namespace Spear.Codec.ProtoBuffer
{
    public static class ProtoBufferExtensions
    {
        public static byte[] Serialize(this object value)
        {
            if (value == null) return new byte[0];

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public static object Deserialize(this byte[] data, Type type)
        {
            if (data == null)
                return null;
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, stream);
            }
        }

        public static T Deserialize<T>(this byte[] data)
        {
            if (data == null)
                return default;
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}
