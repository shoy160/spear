using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Spear.Core.Message;

namespace Spear.Protocol.Tcp.Adapter
{
    public class MicroMessageHandler : ChannelHandlerAdapter
    {
        private readonly IMessageDecoder _messageDecoder;

        public MicroMessageHandler(IMessageDecoder messageDecoder)
        {
            _messageDecoder = messageDecoder;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            var microMessage = _messageDecoder.DecodeAsync<MicroMessage>(data);
            context.FireChannelRead(microMessage);
            ReferenceCountUtil.Release(buffer);
        }
    }
}
