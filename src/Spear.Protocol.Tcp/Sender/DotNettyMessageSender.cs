using DotNetty.Buffers;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Protocol.Tcp.Sender
{
    public abstract class DotNettyMessageSender
    {
        private readonly IMessageEncoder _messageEncoder;

        protected DotNettyMessageSender(IMessageEncoder messageEncoder)
        {
            _messageEncoder = messageEncoder;
        }

        protected IByteBuffer GetByteBuffer(MicroMessage message)
        {
            var data = _messageEncoder.Encode(message);
            return Unpooled.WrappedBuffer(data);
        }
    }
}
