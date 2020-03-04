using DotNetty.Transport.Channels;
using Spear.Core.Message;
using System;
using System.Threading.Tasks;
using Spear.Core;
using Spear.Core.Message.Models;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Tcp.Sender
{
    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    [Protocol(ServiceProtocol.Tcp)]
    public class DotNettyClientSender : DotNettyMessageSender, IMessageSender, IDisposable
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
        public async Task Send(DMessage message, bool flush = true)
        {
            var buffer = await GetByteBuffer(message);
            //Counter.Send(buffer.ReadableBytes);
            if (flush)
            {
                //Counter.Call();
                await _channel.WriteAndFlushAsync(buffer);
            }
            else
                await _channel.WriteAsync(buffer);
        }
    }
}
