using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Core.Runtime.Server
{
    /// <inheritdoc />
    /// <summary> 服务主机 </summary>
    public interface IServiceHost : IDisposable
    {
        /// <summary> 启动主机 </summary>
        /// <param name="endPoint">主机终结点</param>
        /// <returns></returns>
        Task StartAsync(EndPoint endPoint);
    }
}
