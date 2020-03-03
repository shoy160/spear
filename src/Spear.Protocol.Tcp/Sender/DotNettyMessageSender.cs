using System.Threading.Tasks;
using DotNetty.Buffers;
using Spear.Core.Message;

namespace Spear.Protocol.Tcp.Sender
{
    public abstract class DotNettyMessageSender
    {
        private readonly IMessageEncoder _messageEncoder;

        protected DotNettyMessageSender(IMessageEncoder messageEncoder)
        {
            _messageEncoder = messageEncoder;
        }

        protected async Task<IByteBuffer> GetByteBuffer(MicroMessage message)
        {
            var data = await _messageEncoder.EncodeAsync(message);
            return Unpooled.WrappedBuffer(data);
        }
    }
}
