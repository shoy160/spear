using System;
using MessagePack;
using MessagePack.Resolvers;
using Spear.Core.Message.Implementation;

namespace Spear.Codec
{
    public class MessagePackCodec : DMessageCodec
    {
        protected override byte[] OnEncode(object message)
        {
            if (message == null) return new byte[0];
            byte[] buffer;
            if (message.GetType() == typeof(byte[]))
                buffer = (byte[])message;
            else
            {
                //buffer = MessagePackSerializer.Typeless.Serialize(message);
                buffer = MessagePackSerializer.Serialize(message, ContractlessStandardResolver.Options);
            }
            return buffer;
        }

        protected override object OnDecode(byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
                return null;
            //return MessagePackSerializer.Typeless.Deserialize(data);
            return MessagePackSerializer.Deserialize(type, data, ContractlessStandardResolver.Options);
        }
    }
}
