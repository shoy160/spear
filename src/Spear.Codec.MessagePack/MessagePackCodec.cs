using Spear.Codec.MessagePack.Models;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Codec.MessagePack
{
    public class MessagePackCodec : DMessageCodec<MessagePackDynamic, MessagePackInvoke, MessagePackResult>
    {
        public MessagePackCodec(IMessageSerializer serializer) : base(serializer)
        {
        }
    }
}
