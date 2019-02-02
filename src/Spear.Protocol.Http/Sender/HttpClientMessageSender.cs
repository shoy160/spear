using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spear.Protocol.Http.Sender
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpClientMessageSender : IMessageSender
    {
        private readonly IMessageCodecFactory _codecFactory;
        private readonly IMessageListener _messageListener;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private readonly string _url;

        public HttpClientMessageSender(ILogger logger, IHttpClientFactory clientFactory,
            IMessageCodecFactory codecFactory, string url, IMessageListener listener)
        {
            _logger = logger;
            _codecFactory = codecFactory;
            _url = url;
            _messageListener = listener;
            _clientFactory = clientFactory;
        }

        public async Task Send(MicroMessage message, bool flush = true)
        {
            if (!message.IsInvoke)
                return;
            var invoke = message.GetContent<InvokeMessage>();
            var uri = new Uri(new Uri(_url), "micro");
            var client = _clientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, uri.AbsoluteUri);
            if (invoke.Headers != null)
            {
                foreach (var header in invoke.Headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            var data = _codecFactory.GetEncoder().Encode(message);
            req.Content = new ByteArrayContent(data);
            //req.Content = new StringContent(data Encoding.UTF8, "application/json");
            var resp = await client.SendAsync(req);
            var content = await resp.Content.ReadAsByteArrayAsync();
            var result = _codecFactory.GetDecoder().Decode(content);
            await _messageListener.OnReceived(this, result);

        }
    }
}
