using Spear.Core.Message.Implementation;
using System.Threading.Tasks;

namespace Spear.Core.Message
{
    /// <summary>  消息发送者 </summary>
    public interface IMessageSender
    {
        /// <summary> 发送消息 </summary>
        /// <param name="message"></param>
        /// <param name="flush"></param>
        /// <returns></returns>
        Task Send(MicroMessage message, bool flush = true);
    }
}
