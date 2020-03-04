using Spear.Core.Message;
using System.Threading.Tasks;
using Spear.Core.Message.Models;

namespace Spear.Core.Micro
{
    /// <summary> 服务执行者 </summary>
    public interface IMicroExecutor
    {
        /// <summary> 执行 </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Execute(IMessageSender sender, InvokeMessage message);
    }
}
