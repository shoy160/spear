using System;
using Spear.Codec.ProtoBuffer.Models;
using Spear.Core.Message.Implementation;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer
{
    public class ProtoBufferCodec : DMessageCodec
    {
        protected override byte[] OnEncode(object message)
        {
            if (message == null) return new byte[0];
            if (message.GetType() == typeof(byte[]))
                return (byte[])message;
            if (message is InvokeMessage invoke)
            {
                return new ProtoBufferInvoke(invoke).Serialize();
            }

            if (message is ResultMessage result)
            {
                return new ProtoBufferResult(result).Serialize();
            }

            return message.Serialize();
        }

        protected override object OnDecode(byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
                return null;
            if (type == typeof(InvokeMessage))
            {
                var item = data.Deserialize<ProtoBufferInvoke>();
                return item.GetValue();
            }

            if (type == typeof(ResultMessage))
            {
                var item = data.Deserialize<ProtoBufferResult>();
                return item.GetValue();
            }
            return data.Deserialize(type);
        }
    }
}
