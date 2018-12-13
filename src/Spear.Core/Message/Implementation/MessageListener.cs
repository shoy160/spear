using Spear.Core.Message.Implementation;
using System.Threading.Tasks;
using Spear.Core.Message;

namespace Spear.Core.Micro.Implementation
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
