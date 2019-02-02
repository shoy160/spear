using DotNetty.Transport.Channels;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro.Services;
using System.Threading.Tasks;

namespace Spear.Protocol.Tcp.Sender
{
    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    [Protocol(ServiceProtocol.Tcp)]
    public class DotNettyServerSender : DotNettyMessageSender, IMessageSender
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
        public async Task Send(MicroMessage message, bool flush = true)
        {
            var buffer = GetByteBuffer(message);
            if (flush)
                await _context.WriteAndFlushAsync(buffer);
            else
                await _context.WriteAsync(buffer);
        }
    }
}
