using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Sender;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Spear.Protocol.Http
{
    public class HttpClientFactory : IMicroClientFactory
    {
        private readonly ILogger<HttpClientFactory> _logger;
        private readonly IMessageCoderFactory _coderFactory;
        private readonly IMicroExecutor _microExecutor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ConcurrentDictionary<ServiceAddress, Lazy<IMicroClient>> _clients;

        public HttpClientFactory(ILogger<HttpClientFactory> logger, IHttpClientFactory clientFactory, IMessageCoderFactory coderFactory, IMicroExecutor executor = null)
        {
            _coderFactory = coderFactory;
            _microExecutor = executor;
            _logger = logger;
            _clientFactory = clientFactory;
            _clients = new ConcurrentDictionary<ServiceAddress, Lazy<IMicroClient>>();
        }

        public IMicroClient CreateClient(ServiceAddress serviceAddress)
        {
            var lazyClient = _clients.GetOrAdd(serviceAddress, k => new Lazy<IMicroClient>(() =>
                {
                    _logger.LogDebug($"创建客户端：{serviceAddress}创建客户端。");
                    var listener = new MessageListener();
                    var sender = new HttpClientMessageSender(_clientFactory, _coderFactory.GetDecoder(), serviceAddress.ToString(),
                        listener);
                    return new MicroClient(_logger, sender, listener, _microExecutor);
                }
            ));
            return lazyClient.Value;

        }
    }
}
