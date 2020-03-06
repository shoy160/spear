using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketMiddleware : IMiddleware
    {
        private static int _socketCounter;

        // The key is a socket id
        private static readonly ConcurrentDictionary<int, WebSocketClient> Clients = new ConcurrentDictionary<int, WebSocketClient>();

        public CancellationTokenSource SocketLoopTokenSource;

        private static bool _serverIsRunning = true;

        private static CancellationTokenRegistration _appShutdownHandler;

        // use dependency injection to grab a reference to the hosting container's lifetime cancellation tokens
        public WebSocketMiddleware(IHostApplicationLifetime hostLifetime)
        {
            // gracefully close all websockets during shutdown (only register on first instantiation)
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
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        int socketId = Interlocked.Increment(ref _socketCounter);
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        var completion = new TaskCompletionSource<object>();
                        var client = new WebSocketClient(socketId, socket, completion);
                        Clients.TryAdd(socketId, client);
                        Console.WriteLine($"Socket {socketId}: New connection.");

                        // TaskCompletionSource<> is used to keep the middleware pipeline alive;
                        // SocketProcessingLoop calls TrySetResult upon socket termination
                        _ = Task.Run(() => SocketProcessingLoopAsync(client).ConfigureAwait(false));
                        await completion.Task;
                    }
                    else
                    {
                        if (context.Request.Headers["Accept"][0].Contains("text/html"))
                        {
                            Console.WriteLine("Sending HTML to client.");
                            await context.Response.WriteAsync(SimpleHtmlClient.HTML);
                        }
                        else
                        {
                            // ignore other requests (such as favicon)
                            // potentially other middleware will handle it (see finally block)
                        }
                    }
                }
                else
                {
                    // ServerIsRunning = false
                    // HTTP 409 Conflict (with server's current state)
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

        public static void Broadcast(string message)
        {
            Console.WriteLine($"Broadcast: {message}");
            foreach (var kvp in Clients)
                kvp.Value.BroadcastQueue.Add(message);
        }

        // event-handlers are the sole case where async void is valid
        public async void ApplicationShutdownHandler()
        {
            _serverIsRunning = false;
            await CloseAllSocketsAsync();
        }

        private async Task CloseAllSocketsAsync()
        {
            // We can't dispose the sockets until the processing loops are terminated,
            // but terminating the loops will abort the sockets, preventing graceful closing.
            var disposeQueue = new List<System.Net.WebSockets.WebSocket>(Clients.Count);

            while (Clients.Count > 0)
            {
                var client = Clients.ElementAt(0).Value;
                Console.WriteLine($"Closing Socket {client.SocketId}");

                Console.WriteLine("... ending broadcast loop");
                client.BroadcastLoopTokenSource.Cancel();

                if (client.Socket.State != WebSocketState.Open)
                {
                    Console.WriteLine($"... socket not open, state = {client.Socket.State}");
                }
                else
                {
                    var timeout = new CancellationTokenSource(2500);
                    try
                    {
                        Console.WriteLine("... starting close handshake");
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

                Console.WriteLine("... done");
            }

            // now that they're all closed, terminate the blocking ReceiveAsync calls in the SocketProcessingLoop threads
            SocketLoopTokenSource.Cancel();

            // dispose all resources
            foreach (var socket in disposeQueue)
                socket.Dispose();
        }

        private async Task SocketProcessingLoopAsync(WebSocketClient client)
        {
            _ = Task.Run(() => client.BroadcastLoopAsync().ConfigureAwait(false));

            var socket = client.Socket;
            var loopToken = SocketLoopTokenSource.Token;
            var broadcastTokenSource = client.BroadcastLoopTokenSource; // store a copy for use in finally block
            try
            {
                var buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(4096);
                while (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted && !loopToken.IsCancellationRequested)
                {
                    var receiveResult = await client.Socket.ReceiveAsync(buffer, loopToken);
                    // if the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                    if (!loopToken.IsCancellationRequested)
                    {
                        // the client is notifying us that the connection will close; send acknowledgement
                        if (client.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine($"Socket {client.SocketId}: Acknowledging Close frame received from client");
                            broadcastTokenSource.Cancel();
                            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                            // the socket state changes to closed at this point
                        }

                        // echo text or binary data to the broadcast queue
                        if (client.Socket.State == WebSocketState.Open)
                        {
                            Console.WriteLine($"Socket {client.SocketId}: Received {receiveResult.MessageType} frame ({receiveResult.Count} bytes).");
                            Console.WriteLine($"Socket {client.SocketId}: Echoing data to queue.");
                            string message = Encoding.UTF8.GetString(buffer.Array, 0, receiveResult.Count);
                            client.BroadcastQueue.Add(message);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal upon task/token cancellation, disregard
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket {client.SocketId}:");
                //Program.ReportException(ex);
            }
            finally
            {
                broadcastTokenSource.Cancel();

                Console.WriteLine($"Socket {client.SocketId}: Ended processing loop in state {socket.State}");

                // don't leave the socket in any potentially connected state
                if (client.Socket.State != WebSocketState.Closed)
                    client.Socket.Abort();

                // by this point the socket is closed or aborted, the ConnectedClient object is useless
                if (Clients.TryRemove(client.SocketId, out _))
                    socket.Dispose();

                // signal to the middleware pipeline that this task has completed
                client.TaskCompletion.SetResult(true);
            }
        }
    }
}
