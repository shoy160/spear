using Spear.Core.Message;
using Spear.Core.Transport;
using System.Threading.Tasks;

namespace Spear.Core.Runtime.Server
{
    /// <summary> 服务执行器 </summary>
    public interface IServiceExecutor
    {
        /// <summary> 执行 </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">调用消息</param>
        Task ExecuteAsync(IMessageSender sender, TransportMessage message);
    }
}
