using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Sender;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spear.Protocol.Http
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpClientFactory : IMicroClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<HttpClientFactory> _logger;
        private readonly IMessageCodecFactory _codecFactory;
        private readonly IMicroExecutor _microExecutor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ConcurrentDictionary<ServiceAddress, Lazy<Task<IMicroClient>>> _clients;

        public HttpClientFactory(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory,
            IMessageCodecFactory codecFactory, IMicroExecutor executor = null)
        {
            _codecFactory = codecFactory;
            _microExecutor = executor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<HttpClientFactory>();

            _clientFactory = clientFactory;
            _clients = new ConcurrentDictionary<ServiceAddress, Lazy<Task<IMicroClient>>>();
        }

        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        public Task<IMicroClient> CreateClient(ServiceAddress serviceAddress)
        {
            var lazyClient = _clients.GetOrAdd(serviceAddress, k => new Lazy<Task<IMicroClient>>(() =>
                {
                    _logger.LogDebug($"创建客户端：{serviceAddress}创建客户端。");
                    var listener = new MessageListener();
                    var url = serviceAddress.ToString();
                    var sender =
                        new HttpClientMessageSender(_loggerFactory, _clientFactory, _codecFactory, url, listener);
                    IMicroClient client = new MicroClient(sender, listener, _microExecutor, _loggerFactory);
                    return Task.FromResult(client);
                }
            ));
            return lazyClient.Value;

        }
    }
}
