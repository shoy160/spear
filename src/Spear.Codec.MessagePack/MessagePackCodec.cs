using Spear.Codec.MessagePack.Models;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Codec.MessagePack
{
    [Codec(ServiceCodec.MessagePack)]
    public class MessagePackCodec : DMessageCodec<MessagePackDynamic, MessagePackInvoke, MessagePackResult>
    {
        public MessagePackCodec(IMessageSerializer serializer, SpearConfig config = null) : base(serializer, config)
        {
        }
    }
}
