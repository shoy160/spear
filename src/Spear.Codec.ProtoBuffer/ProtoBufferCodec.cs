using System;
using System.IO;
using ProtoBuf;
using Spear.Core.Message.Implementation;

namespace Spear.Codec.ProtoBuffer
{
    public class ProtoBufferCodec : DMessageCodec
    {
        protected override byte[] EncodeInternal(object message)
        {
            if (message == null) return new byte[0];
            byte[] buffer;
            if (message.GetType() == typeof(byte[]))
                buffer = (byte[])message;
            else
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, message);
                    buffer = stream.ToArray();
                }
            }

            //if (compress) buffer = buffer.Zip().Result;
            return buffer;
        }

        protected override object DecodeInternal(byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
                return null;
            //if (compress) data = data.UnZip().Result;
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, stream);
            }
        }
    }
}
