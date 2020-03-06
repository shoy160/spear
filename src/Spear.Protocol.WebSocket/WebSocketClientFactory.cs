using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Message.Models;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.WebSocket
{
    [Protocol(ServiceProtocol.Ws)]
    public class WebSocketClientFactory : DMicroClientFactory
    {
        public WebSocketClientFactory(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory, IMicroExecutor microExecutor = null)
            : base(loggerFactory, codecFactory, microExecutor)
        {
        }

        private void ReceiveMessage(System.Net.WebSockets.WebSocket webSocket, IMessageListener listener, IMessageSender sender)
        {
            Task.Run(async () =>
            {
                var ms = new MemoryStream();
                var buffer = new byte[1024 * 4];
                while (true)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                    await ms.WriteAsync(buffer, 0, buffer.Length);
                    if (result.EndOfMessage)
                    {
                        var resultMessage = await CodecFactory.GetDecoder().DecodeAsync<MessageResult>(ms.ToArray());
                        await listener.OnReceived(sender, resultMessage);
                        ms.Dispose();
                        ms = new MemoryStream();
                    }
                    if (result.CloseStatus.HasValue)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                            CancellationToken.None);
                        Logger.LogInformation($"Client关闭");
                        break;
                    }
                }
            });
        }

        protected override async Task<IMicroClient> Create(ServiceAddress address)
        {
            var listener = new MessageListener();
            var webSocket = new ClientWebSocket();
            var uri = new Uri(new Uri(address.ToString()), "/micro/ws");
            await webSocket.ConnectAsync(uri, CancellationToken.None);
            var sender = new WebSocketMessageSender(webSocket, CodecFactory.GetEncoder());
            ReceiveMessage(webSocket, listener, sender);
            var client = new MicroClient(sender, listener, MicroExecutor, LoggerFactory);
            return client;

        }
    }
}
