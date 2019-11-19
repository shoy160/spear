using Spear.Core.Micro.Services;
using System.Threading.Tasks;
using Spear.Core.Message.Implementation;

namespace Spear.Core.Micro.Implementation
{
    /// <inheritdoc cref="MessageListener" />
    /// <summary> 默认服务监听者 </summary>
    public abstract class MicroListener : MessageListener, IMicroListener
    {
        /// <summary> 开启监听 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        public abstract Task Start(ServiceAddress serviceAddress);

        /// <summary> 开启监听 </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Task Start(string host, int port)
        {
            return Start(new ServiceAddress(host, port));
        }

        /// <summary> 停止监听 </summary>
        /// <returns></returns>
        public abstract Task Stop();
    }
}
