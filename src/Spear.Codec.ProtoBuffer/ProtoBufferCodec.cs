using Spear.Codec.ProtoBuffer.Models;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Codec.ProtoBuffer
{
    public class ProtoBufferCodec : DMessageCodec<ProtoBufferDynamic, ProtoBufferInvoke, ProtoBufferResult>
    {
        public ProtoBufferCodec(IMessageSerializer serializer) : base(serializer)
        {
        }
    }
}
