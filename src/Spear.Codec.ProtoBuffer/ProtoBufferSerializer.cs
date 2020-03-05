using System;
using System.IO;
using ProtoBuf;
using Spear.Core.Message;

namespace Spear.Codec.ProtoBuffer
{
    public class ProtoBufferSerializer : IMessageSerializer
    {
        public byte[] Serialize(object value)
        {
            if (value == null) return new byte[0];

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public byte[] SerializeNoType(object value)
        {
            return Serialize(value);
        }

        public object Deserialize(byte[] data, Type type)
        {
            if (data == null)
                return null;
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, stream);
            }
        }

        public object DeserializeNoType(byte[] data, Type type)
        {
            return Deserialize(data, type);
        }

        public T Deserialize<T>(byte[] data)
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
