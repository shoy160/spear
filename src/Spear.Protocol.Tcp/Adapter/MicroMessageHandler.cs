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
        private readonly bool _gzip;

        public MicroMessageHandler(IMessageDecoder messageDecoder, bool gzip)
        {
            _messageDecoder = messageDecoder;
            _gzip = gzip;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);

            //Counter.Received(buffer.ReadableBytes);
            var microMessage = _messageDecoder.Decode<T>(data, _gzip);
            context.FireChannelRead(microMessage);
            ReferenceCountUtil.Release(buffer);
        }
    }
}
