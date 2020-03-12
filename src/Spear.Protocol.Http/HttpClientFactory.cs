using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Sender;

namespace Spear.Protocol.Http
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpClientFactory : DMicroClientFactory
    {

        public HttpClientFactory(ILoggerFactory loggerFactory, IServiceProvider provider, IMicroExecutor executor = null)
            : base(loggerFactory, provider, executor)
        {
        }

        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        protected override Task<IMicroClient> Create(ServiceAddress serviceAddress)
        {
            Logger.LogDebug($"创建客户端：{serviceAddress}创建客户端。");
            var listener = new MessageListener();
            var sender = new HttpClientMessageSender(Provider, serviceAddress, listener);
            IMicroClient client = new MicroClient(sender, listener, MicroExecutor, LoggerFactory);
            return Task.FromResult(client);
        }
    }
}
