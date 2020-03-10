using System.Threading.Tasks;

namespace Spear.Protocol.WebSocket
{
    public class WebSocketClient
    {
        public WebSocketClient(int socketId, System.Net.WebSockets.WebSocket socket, TaskCompletionSource<object> taskCompletion)
        {
            SocketId = socketId;
            Socket = socket;
            TaskCompletion = taskCompletion;
        }

        public int SocketId { get; }

        public System.Net.WebSockets.WebSocket Socket { get; }

        public TaskCompletionSource<object> TaskCompletion { get; }
    }
}
