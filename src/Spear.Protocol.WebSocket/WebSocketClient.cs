using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketClient
    {
        private readonly ILogger<WebSocketClient> _logger;
        public WebSocketClient(System.Net.WebSockets.WebSocket socket, ILoggerFactory loggerFactory, TaskCompletionSource<object> taskCompletion)
        {
            SocketId = Guid.NewGuid().ToString("N");
            Socket = socket;
            TaskCompletion = taskCompletion;
            _logger = loggerFactory.CreateLogger<WebSocketClient>();
        }

        public string SocketId { get; }

        public System.Net.WebSockets.WebSocket Socket { get; }

        public TaskCompletionSource<object> TaskCompletion { get; }

        public event Action<string, System.Net.WebSockets.WebSocket> OnClose;

        public event Action<byte[]> OnReceive;


        public async Task ReceiveAsync(CancellationToken loopToken)
        {
            try
            {
                var stream = new MemoryStream();
                var buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(4096);
                while (Socket.State != WebSocketState.Closed && Socket.State != WebSocketState.Aborted && !loopToken.IsCancellationRequested)
                {
                    var result = await Socket.ReceiveAsync(buffer, loopToken);
                    if (loopToken.IsCancellationRequested)
                        continue;
                    if (Socket.State == WebSocketState.CloseReceived && result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation($"Socket {SocketId}: Acknowledging Close frame received from client");
                        TaskCompletion.SetCanceled();
                        await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    }

                    if (Socket.State == WebSocketState.Open)
                    {
                        await stream.WriteAsync(buffer.Array, buffer.Offset, result.Count - buffer.Offset,
                            loopToken);
                        stream.Seek(0, SeekOrigin.Begin);
                        if (!result.EndOfMessage)
                            continue;
                        OnReceive?.Invoke(stream.ToArray());
                        await stream.DisposeAsync();
                        stream = new MemoryStream();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Socket {SocketId}:");
            }
            finally
            {
                TaskCompletion.SetCanceled();

                _logger.LogInformation($"Socket {SocketId}: 停止服务，状态码 {Socket.State}");

                if (Socket.State != WebSocketState.Closed)
                    Socket.Abort();
                OnClose?.Invoke(SocketId, Socket);
                //if (Clients.TryRemove(SocketId, out _))
                Socket.Dispose();
                TaskCompletion.SetResult(true);
            }
        }
    }
}
