using DotNetty.Transport.Channels;
using Spear.Core.Message;
using Spear.Core.Transport;
using System.Threading.Tasks;

namespace Spear.DotNetty.Sender
{
    public class DotNettyServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;
        public DotNettyServerMessageSender(IMessageEncoder transportMessageEncoder, IChannelHandlerContext context)
            : base(transportMessageEncoder)
        {
            _context = context;
        }

        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAsync(buffer);
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAndFlushAsync(buffer);
        }
    }
}
