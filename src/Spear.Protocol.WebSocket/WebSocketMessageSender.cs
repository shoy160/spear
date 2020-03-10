using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketMessageSender : IMessageSender
    {
        private readonly System.Net.WebSockets.WebSocket _webSocket;
        private readonly IMessageEncoder _messageEncoder;

        public WebSocketMessageSender(System.Net.WebSockets.WebSocket webSocket, IMessageEncoder messageEncoder)
        {
            _webSocket = webSocket;
            _messageEncoder = messageEncoder;
        }

        public async Task Send(DMessage message, bool flush = true)
        {
            var buffer = await _messageEncoder.EncodeAsync(message);
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                throw ErrorCodes.SystemError.CodeException($"WebSocket连接已关闭");
            await _webSocket.SendAsync(new ReadOnlyMemory<byte>(buffer), WebSocketMessageType.Binary, flush,
                CancellationToken.None);
        }
    }
}
