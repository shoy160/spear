using Spear.Core.Micro.Services;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    /// <inheritdoc cref="MessageListener" />
    /// <summary> 默认服务监听者 </summary>
    public abstract class MicroListener : MessageListener, IMicroListener
    {
        public abstract Task Start(ServiceAddress serviceAddress);

        public Task Start(string host, int port)
        {
            return Start(new ServiceAddress(host, port));
        }

        public abstract Task Stop();
    }
}
