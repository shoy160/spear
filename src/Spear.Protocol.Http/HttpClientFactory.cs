using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
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
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory,
            IMessageCodecFactory codecFactory, IMicroExecutor executor = null)
            : base(loggerFactory, codecFactory, executor)
        {
            _clientFactory = clientFactory;
        }

        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        protected override Task<IMicroClient> Create(ServiceAddress serviceAddress)
        {
            Logger.LogDebug($"创建客户端：{serviceAddress}创建客户端。");
            var listener = new MessageListener();
            var url = serviceAddress.ToString();
            var sender = new HttpClientMessageSender(LoggerFactory, _clientFactory, CodecFactory, url, listener);
            IMicroClient client = new MicroClient(sender, listener, MicroExecutor, LoggerFactory);
            return Task.FromResult(client);
        }
    }
}
