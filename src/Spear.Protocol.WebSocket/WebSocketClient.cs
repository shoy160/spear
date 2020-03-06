using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketClient
    {
        public const int BroadcastTransmitIntervalMs = 250;
        public WebSocketClient(int socketId, System.Net.WebSockets.WebSocket socket, TaskCompletionSource<object> taskCompletion)
        {
            SocketId = socketId;
            Socket = socket;
            TaskCompletion = taskCompletion;
        }

        public int SocketId { get; private set; }

        public System.Net.WebSockets.WebSocket Socket { get; private set; }

        public TaskCompletionSource<object> TaskCompletion { get; private set; }

        public BlockingCollection<string> BroadcastQueue { get; } = new BlockingCollection<string>();

        public CancellationTokenSource BroadcastLoopTokenSource { get; set; } = new CancellationTokenSource();

        public async Task BroadcastLoopAsync()
        {
            var cancellationToken = BroadcastLoopTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(BroadcastTransmitIntervalMs, cancellationToken);
                    if (!cancellationToken.IsCancellationRequested && Socket.State == WebSocketState.Open && BroadcastQueue.TryTake(out var message))
                    {
                        Console.WriteLine($"Socket {SocketId}: Sending from queue.");
                        var msgbuf = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                        await Socket.SendAsync(msgbuf, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                    }
                }
                catch (OperationCanceledException)
                {
                    // normal upon task/token cancellation, disregard
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
