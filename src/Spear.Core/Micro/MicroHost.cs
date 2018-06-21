using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Core.Micro
{
    public class MicroHost : DMicroHost
    {
        private readonly Func<EndPoint, Task<IMicroListener>> _messageListenerFactory;
        private IMicroListener _serverMessageListener;

        public MicroHost(Func<EndPoint, Task<IMicroListener>> messageListenerFactory, IMicroExecutor serviceExecutor) :
            base(serviceExecutor)
        {
            _messageListenerFactory = messageListenerFactory;
        }

        public override void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }

        /// <inheritdoc />
        /// <summary> 启动微服务 </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override async Task Start(EndPoint endPoint)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(endPoint);
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MicroListener.OnReceived(sender, message);
                });
            };
        }
    }
}
