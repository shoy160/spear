using DotNetty.Transport.Channels;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Models;
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
        private readonly ServiceAddress _address;

        public DotNettyServerSender(IMessageEncoder messageEncoder, IChannelHandlerContext context, ServiceAddress address)
            : base(messageEncoder)
        {
            _context = context;
            _address = address;
        }

        /// <summary> 发送消息 </summary>
        /// <param name="message">消息内容</param>
        /// <param name="flush">清空缓冲池</param>
        /// <returns>一个任务。</returns>
        public async Task Send(DMessage message, bool flush = true)
        {
            var buffer = await GetByteBuffer(message, _address.Gzip);
            if (flush)
                await _context.WriteAndFlushAsync(buffer);
            else
                await _context.WriteAsync(buffer);
        }
    }
}
