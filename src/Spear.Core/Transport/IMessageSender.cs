using System.Threading.Tasks;
using Spear.Core.Message;

namespace Spear.Core.Transport
{
    /// <summary> 消息发送者 </summary>
    public interface IMessageSender
    {
        /// <summary> 发送消息 </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendAsync(TransportMessage message);

        /// <summary> 发送消息并清空缓冲区 </summary>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task SendAndFlushAsync(TransportMessage message);
    }
}
