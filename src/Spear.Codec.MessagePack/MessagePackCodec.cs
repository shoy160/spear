using System;
using MessagePack;
using MessagePack.Resolvers;
using Spear.Codec.MessagePack.Models;
using Spear.Core.Message.Implementation;
using Spear.Core.Message.Models;

namespace Spear.Codec
{
    public class MessagePackCodec : DMessageCodec
    {
        protected override byte[] OnEncode(object message)
        {
            if (message == null) return new byte[0];

            if (message.GetType() == typeof(byte[]))
                return (byte[])message;

            if (message is InvokeMessage invoke)
            {
                return new MessagePackInvoke(invoke).Serialize();
            }

            if (message is ResultMessage result)
            {
                return new MessagePackResult(result).Serialize();
            }

            return MessagePackSerializer.Serialize(message, ContractlessStandardResolver.Options);
        }

        protected override object OnDecode(byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
                return null;
            if (type == typeof(InvokeMessage))
            {
                var item = data.Deserialize<MessagePackInvoke>();
                return item.GetValue();
            }

            if (type == typeof(ResultMessage))
            {
                var item = data.Deserialize<MessagePackResult>();
                return item.GetValue();
            }
            return MessagePackSerializer.Deserialize(type, data, ContractlessStandardResolver.Options);
        }
    }
}
