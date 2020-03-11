using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Http.Sender
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpClientMessageSender : IMessageSender
    {
        private readonly IMessageCodecFactory _codecFactory;
        private readonly IMessageListener _messageListener;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<HttpClientMessageSender> _logger;
        private readonly ServiceAddress _address;

        public HttpClientMessageSender(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory,
            IMessageCodecFactory codecFactory, ServiceAddress address, IMessageListener listener)
        {
            _logger = loggerFactory.CreateLogger<HttpClientMessageSender>();
            _codecFactory = codecFactory;
            _address = address;
            _messageListener = listener;
            _clientFactory = clientFactory;
        }

        public async Task Send(DMessage message, bool flush = true)
        {
            if (!(message is InvokeMessage invokeMessage))
                return;
            var uri = new Uri(new Uri(_address.ToString()), "micro/executor");
            var client = _clientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, uri.AbsoluteUri);
            if (invokeMessage.Headers != null)
            {
                foreach (var header in invokeMessage.Headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            var data = await _codecFactory.GetEncoder().EncodeAsync(message, _address.Gzip);
            req.Content = new ByteArrayContent(data);
            if (_address.Gzip)
            {
                req.Content.Headers.ContentEncoding.Add("gzip");
            }
            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                throw new SpearException($"服务请求异常，状态码{(int)resp.StatusCode}");
            }
            var content = await resp.Content.ReadAsByteArrayAsync();
            var result = await _codecFactory.GetDecoder().DecodeAsync<MessageResult>(content, _address.Gzip);
            await _messageListener.OnReceived(this, result);

        }
    }
}
