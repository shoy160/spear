using Spear.Core.Message;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Core.Micro
{
    public abstract class DMicroHost : IMicroHost
    {

        private readonly IMicroExecutor _microExecutor;

        /// <summary>
        /// 消息监听者。
        /// </summary>
        protected IMicroListener MicroListener { get; } = new MicroListener();

        protected DMicroHost(IMicroExecutor microExecutor)
        {
            _microExecutor = microExecutor;
            MicroListener.Received += MessageListener_Received;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public abstract void Dispose();

        /// <inheritdoc />
        /// <summary> 启动微服务 </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public abstract Task Start(EndPoint endPoint);

        private async Task MessageListener_Received(IMicroSender sender, MicroMessage message)
        {
            await _microExecutor.Execute(sender, message);
        }
    }
}
