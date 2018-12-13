using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Spear.Core.Message;
using Spear.Core.Micro;
using System;
using System.Threading.Tasks;

namespace Spear.DotNetty
{
    public abstract class DotNettyMicroSender
    {
        private readonly IMessageEncoder _messageEncoder;

        protected DotNettyMicroSender(IMessageEncoder messageEncoder)
        {
            _messageEncoder = messageEncoder;
        }

        protected IByteBuffer GetByteBuffer(IMicroMessage message)
        {
            var data = _messageEncoder.Encode(message);
            return Unpooled.WrappedBuffer(data);
        }
    }

    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    public class DotNettyClientSender : DotNettyMicroSender, IMessageSender, IDisposable
    {
        private readonly IChannel _channel;

        public DotNettyClientSender(IMessageEncoder messageEncoder, IChannel channel) : base(messageEncoder)
        {
            _channel = channel;
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        /// <summary> 发送消息 </summary>
        /// <param name="message">消息内容</param>
        /// <param name="flush"></param>
        /// <returns>一个任务。</returns>
        public async Task Send(IMicroMessage message, bool flush = true)
        {
            var buffer = GetByteBuffer(message);
            if (flush)
                await _channel.WriteAndFlushAsync(buffer);
            else
                await _channel.WriteAsync(buffer);
        }
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyServerSender : DotNettyMicroSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public DotNettyServerSender(IMessageEncoder messageEncoder, IChannelHandlerContext context)
            : base(messageEncoder)
        {
            _context = context;
        }

        /// <summary> 发送消息 </summary>
        /// <param name="message">消息内容</param>
        /// <param name="flush">清空缓冲池</param>
        /// <returns>一个任务。</returns>
        public async Task Send(IMicroMessage message, bool flush = true)
        {
            var buffer = GetByteBuffer(message);
            if (flush)
                await _context.WriteAndFlushAsync(buffer);
            else
                await _context.WriteAsync(buffer);
        }
    }
}
