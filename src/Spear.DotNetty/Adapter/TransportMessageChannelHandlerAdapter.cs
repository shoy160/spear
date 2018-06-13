using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Spear.Core.Transport;

namespace Spear.DotNetty.Adapter
{
    internal class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly IMessageDecoder _messageDecoder;

        public TransportMessageChannelHandlerAdapter(IMessageDecoder messageDecoder)
        {
            _messageDecoder = messageDecoder;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            var transportMessage = _messageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
            ReferenceCountUtil.Release(buffer);
        }
    }
}
