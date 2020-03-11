using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;

namespace Spear.Protocol.WebSocket.Sender
{
    public class WebSocketMessageSender : IMessageSender
    {
        private readonly System.Net.WebSockets.WebSocket _webSocket;
        private readonly IMessageEncoder _messageEncoder;
        private readonly bool _gzip;

        public WebSocketMessageSender(System.Net.WebSockets.WebSocket webSocket, IMessageEncoder messageEncoder, bool gzip)
        {
            _webSocket = webSocket;
            _messageEncoder = messageEncoder;
            _gzip = gzip;
        }

        public async Task Send(DMessage message, bool flush = true)
        {
            var buffer = await _messageEncoder.EncodeAsync(message, _gzip);
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                throw ErrorCodes.SystemError.CodeException($"WebSocket连接已关闭");
            await _webSocket.SendAsync(new ReadOnlyMemory<byte>(buffer), WebSocketMessageType.Binary, flush,
                CancellationToken.None);
        }
    }
}
