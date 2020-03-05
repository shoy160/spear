using ProtoBuf;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferDynamic : DMessageDynamic
    {
        [ProtoMember(1)]
        public override string ContentType { get; set; }

        [ProtoMember(2)]
        public override byte[] Content { get; set; }

        public ProtoBufferDynamic() : base(new ProtoBufferSerializer())
        {
        }
    }
}
