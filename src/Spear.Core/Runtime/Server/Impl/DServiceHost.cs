using Spear.Core.Transport;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Core.Runtime.Server.Impl
{
    public class DServiceHost : ServiceHost
    {
        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;
        private IMessageListener _serverMessageListener;

        public DServiceHost(Func<EndPoint, Task<IMessageListener>> messageListenerFactory, IServiceExecutor serviceExecutor)
            : base(serviceExecutor)
        {
            _messageListenerFactory = messageListenerFactory;
        }

        public override void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }

        public override async Task StartAsync(EndPoint endPoint)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(endPoint);
            _serverMessageListener.Received += async (sender, message) =>
            {
                await Task.Run(() =>
                {
                    MessageListener.OnReceived(sender, message);
                });
            };
        }
    }
}
