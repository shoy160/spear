using System.Threading.Tasks;

namespace Spear.Core.Message.Implementation
{
    /// <summary> 消息监听者 </summary>
    public class MessageListener : IMessageListener
    {
        /// <inheritdoc />
        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        public event ReceivedDelegate Received;

        public async Task OnReceived(IMessageSender sender, MicroMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }
    }
}
