using Spear.Core.Micro.Services;
using System.Threading.Tasks;

namespace Spear.Core.Micro
{
    /// <summary> 微服务主机 </summary>
    public interface IMicroHost
    {
        /// <summary> 启动服务 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        Task Start(ServiceAddress serviceAddress);

        /// <summary> 启动服务 </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        Task Start(string ip, int port);

        /// <summary> 停止服务 </summary>
        /// <returns></returns>
        Task Stop();
    }
}
