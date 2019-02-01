using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Message;
using Spear.Core.Micro.Services;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    public abstract class DMicroHost : IMicroHost
    {
        private readonly IMicroExecutor _microExecutor;
        private readonly ILogger<DMicroHost> _logger;

        /// <summary> 消息监听者。 </summary>
        protected IMicroListener MicroListener { get; set; }

        protected DMicroHost(ILogger<DMicroHost> logger, IMicroExecutor microExecutor, IMicroListener microListener)
        {
            _logger = logger;
            _microExecutor = microExecutor;
            MicroListener = microListener;
            MicroListener.Received += MessageListenerReceived;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose()
        {
            Task.Run(async () => await Stop()).Wait();
        }

        /// <summary> 启动微服务 </summary>
        /// <param name="serviceAddress">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public abstract Task Start(ServiceAddress serviceAddress);

        public Task Start(string host, int port)
        {
            return Start(new ServiceAddress(host, port));
        }

        public abstract Task Stop();

        private async Task MessageListenerReceived(IMessageSender sender, MicroMessage message)
        {
            _logger.LogDebug($"receive:{JsonConvert.SerializeObject(message)}");
            await _microExecutor.Execute(sender, message);
        }
    }
}
