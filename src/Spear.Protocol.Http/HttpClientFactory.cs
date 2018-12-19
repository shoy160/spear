using Acb.Core.Logging;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Sender;
using System;
using System.Collections.Concurrent;

namespace Spear.Protocol.Http
{
    public class HttpClientFactory : IMicroClientFactory
    {
        private readonly ILogger _logger;
        private readonly IMessageCoderFactory _coderFactory;
        private readonly IMicroExecutor _microExecutor;
        private readonly ConcurrentDictionary<ServiceAddress, Lazy<IMicroClient>> _clients;

        public HttpClientFactory(IMessageCoderFactory coderFactory, IMicroExecutor executor = null)
        {
            _coderFactory = coderFactory;
            _microExecutor = executor;
            _clients = new ConcurrentDictionary<ServiceAddress, Lazy<IMicroClient>>();
            _logger = LogManager.Logger<HttpClientFactory>();
        }

        public IMicroClient CreateClient(ServiceAddress serviceAddress)
        {
            var lazyClient = _clients.GetOrAdd(serviceAddress, k => new Lazy<IMicroClient>(() =>
                {
                    _logger.Debug($"创建客户端：{serviceAddress}创建客户端。");
                    var listener = new MessageListener();
                    var sender = new HttpClientMessageSender(_coderFactory.GetDecoder(), serviceAddress.ToString(),
                        listener);
                    return new MicroClient(sender, listener, _microExecutor);
                }
            ));
            return lazyClient.Value;

        }
    }
}
