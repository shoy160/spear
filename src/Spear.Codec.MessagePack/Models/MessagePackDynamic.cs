using MessagePack;
using Spear.Core.Message.Models;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackDynamic : DMessageDynamic
    {
        [Key(0)]
        public override string ContentType { get; set; }

        [Key(1)]
        public override byte[] Content { get; set; }

        public MessagePackDynamic() : base(new MessagePackMessageSerializer()) { }
    }
}
