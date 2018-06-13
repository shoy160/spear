using Spear.Core.Message;
using System.Threading.Tasks;

namespace Spear.Core.Transport.Impl
{
    /// <summary> 默认的消息监听者 </summary>
    public class MessageListener : IMessageListener
    {
        /// <summary> 接收消息事件 </summary>
        public event ReceivedDelegate Received;

        /// <summary> 接收到消息 </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null) return;
            await Received(sender, message);
        }
    }
}
