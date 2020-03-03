using System;
using Spear.Core.Message.Implementation;

namespace Spear.Codec
{
    public class MessagePackCodec : DMessageCodec
    {
        protected override byte[] EncodeInternal(object message)
        {
            if (message == null) return new byte[0];
            byte[] buffer;
            if (message.GetType() == typeof(byte[]))
                buffer = (byte[])message;
            else
            {
                buffer = MessagePack.MessagePackSerializer.Serialize(message);
            }

            //buffer = buffer.Zip().Result;
            return buffer;
        }

        protected override object DecodeInternal(byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
                return null;
            return MessagePack.MessagePackSerializer.Deserialize(type, data);
        }
    }
}
