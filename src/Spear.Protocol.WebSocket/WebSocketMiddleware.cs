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

namespace Spear.Protocol.WebSocket
{
    public class WebSocketMiddleware : IMiddleware
    {
        private static int _socketCounter;

        private static readonly ConcurrentDictionary<int, WebSocketClient> Clients = new ConcurrentDictionary<int, WebSocketClient>();

        public CancellationTokenSource SocketLoopTokenSource;

        private static bool _serverIsRunning = true;

        private static CancellationTokenRegistration _appShutdownHandler;
        private readonly IMessageListener _listener;
        private readonly IMessageCodecFactory _codecFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(IMessageListener listener, IHostApplicationLifetime hostLifetime,
            IMessageCodecFactory codecFactory, ILoggerFactory loggerFactory)
        {
            _listener = listener;
            _codecFactory = codecFactory;
            _loggerFactory = loggerFactory;
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
                        var socketId = Interlocked.Increment(ref _socketCounter);
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        var completion = new TaskCompletionSource<object>();
                        var client = new WebSocketClient(socketId, socket, completion);

                        Clients.TryAdd(socketId, client);
                        _logger.LogInformation($"Socket {socketId}: New connection.");

                        // TaskCompletionSource<> is used to keep the middleware pipeline alive;
                        // SocketProcessingLoop calls TrySetResult upon socket termination
                        _ = Task.Run(() => SocketProcessingLoopAsync(client).ConfigureAwait(false));
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

        private async Task SocketProcessingLoopAsync(WebSocketClient client)
        {
            var socket = client.Socket;
            var loopToken = SocketLoopTokenSource.Token;
            try
            {
                var stream = new MemoryStream();
                var buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(4096);
                while (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted && !loopToken.IsCancellationRequested)
                {
                    var receiveResult = await client.Socket.ReceiveAsync(buffer, loopToken);
                    if (loopToken.IsCancellationRequested)
                        continue;
                    if (client.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation($"Socket {client.SocketId}: Acknowledging Close frame received from client");
                        client.TaskCompletion.SetCanceled();
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                    }

                    if (client.Socket.State == WebSocketState.Open)
                    {
                        await stream.WriteAsync(buffer.ToArray(), loopToken);
                        if (!receiveResult.EndOfMessage)
                            continue;
                        var invokeMessage = await _codecFactory.GetDecoder()
                            .DecodeAsync<InvokeMessage>(stream.ToArray());
                        var sender = new WebSocketMessageSender(client.Socket, _codecFactory.GetEncoder());
                        await _listener.OnReceived(sender, invokeMessage);
                        await stream.DisposeAsync();
                        stream = new MemoryStream();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Socket {client.SocketId}:");
            }
            finally
            {
                client.TaskCompletion.SetCanceled();

                _logger.LogInformation($"Socket {client.SocketId}: 停止服务，状态码 {socket.State}");

                if (client.Socket.State != WebSocketState.Closed)
                    client.Socket.Abort();

                if (Clients.TryRemove(client.SocketId, out _))
                    socket.Dispose();
                client.TaskCompletion.SetResult(true);
            }
        }
    }
}
