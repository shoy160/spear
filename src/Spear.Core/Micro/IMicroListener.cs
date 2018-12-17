using Spear.Core.Micro.Services;
using System.Threading.Tasks;
using Spear.Core.Message;

namespace Spear.Core.Micro
{
    /// <summary> 微服务监听者 </summary>
    public interface IMicroListener : IMessageListener
    {
        /// <summary> 启动监听 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        Task Start(ServiceAddress serviceAddress);

        Task Start(string host, int port);

        /// <summary> 停止监听 </summary>
        /// <returns></returns>
        Task Stop();
    }
}
