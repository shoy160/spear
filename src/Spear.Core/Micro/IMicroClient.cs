using System.Threading.Tasks;
using Spear.Core.Message;

namespace Spear.Core.Micro
{
    /// <summary> 微服务客户端 </summary>
    public interface IMicroClient
    {
        /// <summary> 发送消息 </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用消息的传输消息。</returns>
        Task<T> Send<T>(object message);

        Task<ResultMessage> Send(InvokeMessage message);
    }
}
