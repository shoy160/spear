using DotNetty.Buffers;
using Spear.Core.Message;
using Spear.Core.Transport;

namespace Spear.DotNetty.Sender
{
    public abstract class DotNettyMessageSender
    {
        private readonly IMessageEncoder _transportMessageEncoder;

        protected DotNettyMessageSender(IMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message);
            return Unpooled.WrappedBuffer(data);
        }
    }
}
