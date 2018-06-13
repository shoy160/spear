using DotNetty.Transport.Channels;
using Spear.Core.Message;
using Spear.Core.Transport;
using System;
using System.Threading.Tasks;

namespace Spear.DotNetty.Sender
{
    public class DotNettyClientMessageSender : DotNettyMessageSender, IMessageSender, IDisposable
    {
        private readonly IChannel _channel;
        public DotNettyClientMessageSender(IMessageEncoder transportMessageEncoder, IChannel channel)
            : base(transportMessageEncoder)
        {
            _channel = channel;
        }

        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }
    }
}
