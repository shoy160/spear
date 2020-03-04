using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Spear.Core.Message;
using Spear.Core.Message.Models;

namespace Spear.Protocol.Tcp.Adapter
{
    public class MicroMessageHandler<T> : ChannelHandlerAdapter
        where T : DMessage
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

            //Counter.Received(buffer.ReadableBytes);

            var microMessage = _messageDecoder.Decode<T>(data);
            context.FireChannelRead(microMessage);
            ReferenceCountUtil.Release(buffer);
        }
    }
}
