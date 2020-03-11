using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Micro.Services;
using Spear.Protocol.WebSocket.Sender;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketMiddleware : IMiddleware
    {
        private static readonly ConcurrentDictionary<string, WebSocketClient> Clients = new ConcurrentDictionary<string, WebSocketClient>();

        public CancellationTokenSource SocketLoopTokenSource;

        private static bool _serverIsRunning = true;

        private static CancellationTokenRegistration _appShutdownHandler;
        private readonly IMessageListener _listener;
        private readonly IMessageCodecFactory _codecFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WebSocketMiddleware> _logger;
        private readonly ServiceAddress _address;

        public WebSocketMiddleware(IMessageListener listener, IHostApplicationLifetime hostLifetime,
            IMessageCodecFactory codecFactory, ILoggerFactory loggerFactory, ServiceAddress address)
        {
            _listener = listener;
            _codecFactory = codecFactory;
            _loggerFactory = loggerFactory;
            _address = address;
            _logger = loggerFactory.CreateLogger<WebSocketMiddleware>();
            if (_appShutdownHandler.Token.Equals(CancellationToken.None))
                _appShutdownHandler = hostLifetime.ApplicationStopping.Register(ApplicationShutdownHandler);

            SocketLoopTokenSource = new CancellationTokenSource();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                if (_serverIsRunning)
                {
                    if (context.Request.Path == "/micro/ws" && context.WebSockets.IsWebSocketRequest)
                    {
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        var completion = new TaskCompletionSource<object>();

                        var client = new WebSocketClient(socket, _loggerFactory, completion);

                        client.OnClose += (key, websocket) => Clients.TryRemove(key, out _);
                        client.OnReceive += async buffer =>
                        {
                            var invokeMessage = await _codecFactory.GetDecoder()
                                .DecodeAsync<InvokeMessage>(buffer, _address.Gzip);
                            var sender = new WebSocketMessageSender(client.Socket, _codecFactory.GetEncoder(), _address.Gzip);
                            await _listener.OnReceived(sender, invokeMessage);
                        };
                        Clients.TryAdd(client.SocketId, client);
                        _logger.LogInformation($"Socket {client.SocketId}: New connection.");

                        // TaskCompletionSource<> is used to keep the middleware pipeline alive;
                        // SocketProcessingLoop calls TrySetResult upon socket termination
                        _ = Task.Run(() => client.ReceiveAsync(SocketLoopTokenSource.Token).ConfigureAwait(false));
                        await completion.Task;
                    }
                }
                else
                {
                    context.Response.StatusCode = 409;
                }
            }
            catch (Exception ex)
            {
                // HTTP 500 Internal server error
                context.Response.StatusCode = 500;
                //Program.ReportException(ex);
            }
            finally
            {
                // if this middleware didn't handle the request, pass it on
                if (!context.Response.HasStarted)
                    await next(context);
            }
        }

        // event-handlers are the sole case where async void is valid
        public async void ApplicationShutdownHandler()
        {
            _serverIsRunning = false;
            await CloseAllSocketsAsync();
        }

        private async Task CloseAllSocketsAsync()
        {
            var disposeQueue = new List<System.Net.WebSockets.WebSocket>(Clients.Count);

            while (Clients.Count > 0)
            {
                var client = Clients.ElementAt(0).Value;
                _logger.LogInformation($"Closing Socket {client.SocketId}");

                _logger.LogInformation("... ending broadcast loop");
                client.TaskCompletion.SetCanceled();

                if (client.Socket.State != WebSocketState.Open)
                {
                    _logger.LogInformation($"... socket not open, state = {client.Socket.State}");
                }
                else
                {
                    var timeout = new CancellationTokenSource(2500);
                    try
                    {
                        _logger.LogInformation("... starting close handshake");
                        await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        //Program.ReportException(ex);
                        // normal upon task/token cancellation, disregard
                    }
                }

                if (Clients.TryRemove(client.SocketId, out _))
                {
                    // only safe to Dispose once, so only add it if this loop can't process it again
                    disposeQueue.Add(client.Socket);
                }
            }

            // now that they're all closed, terminate the blocking ReceiveAsync calls in the SocketProcessingLoop threads
            SocketLoopTokenSource.Cancel();

            // dispose all resources
            foreach (var socket in disposeQueue)
                socket.Dispose();
        }
    }
}
