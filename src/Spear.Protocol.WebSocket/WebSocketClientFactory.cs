using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Message.Models;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.WebSocket.Sender;

namespace Spear.Protocol.WebSocket
{
    [Protocol(ServiceProtocol.Ws)]
    public class WebSocketClientFactory : DMicroClientFactory
    {
        public WebSocketClientFactory(ILoggerFactory loggerFactory, IServiceProvider provider, IMicroExecutor microExecutor = null)
            : base(loggerFactory, provider, microExecutor)
        {
        }

        protected override async Task<IMicroClient> Create(ServiceAddress address)
        {
            var listener = new MessageListener();
            var webSocket = new ClientWebSocket();
            var uri = new Uri(new Uri(address.ToString()), "/micro/ws");
            await webSocket.ConnectAsync(uri, CancellationToken.None);
            var codec = Provider.GetClientCodec(address.Codec);
            var sender = new WebSocketMessageSender(webSocket, codec, address.Gzip);

            var completion = new TaskCompletionSource<object>();
            var socketClient = new WebSocketClient(webSocket, LoggerFactory, completion);
            socketClient.OnReceive += async buffer =>
            {
                var resultMessage = await codec.DecodeAsync<MessageResult>(buffer, address.Gzip);
                await listener.OnReceived(sender, resultMessage);
            };
            socketClient.OnClose += (key, socket) => Remove(address);

            _ = Task.Run(() => socketClient.ReceiveAsync(CancellationToken.None).ConfigureAwait(false));
            return new MicroClient(sender, listener, MicroExecutor, LoggerFactory);
        }
    }
}
