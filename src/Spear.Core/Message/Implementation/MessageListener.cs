using System.Threading.Tasks;
using Spear.Core.Message.Models;

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

        /// <summary> 接收到消息 </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnReceived(IMessageSender sender, DMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }
    }
}
