using Spear.Codec.ProtoBuffer.Models;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Codec.ProtoBuffer
{
    [Codec(ServiceCodec.ProtoBuf)]
    public class ProtoBufferCodec : DMessageCodec<ProtoBufferDynamic, ProtoBufferInvoke, ProtoBufferResult>
    {
        public ProtoBufferCodec(IMessageSerializer serializer, SpearConfig config = null) : base(serializer, config)
        {
        }
    }
}
