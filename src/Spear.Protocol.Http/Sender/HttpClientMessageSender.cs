using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
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
        private readonly string _url;

        public HttpClientMessageSender(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory,
            IMessageCodecFactory codecFactory, string url, IMessageListener listener)
        {
            _logger = loggerFactory.CreateLogger<HttpClientMessageSender>();
            _codecFactory = codecFactory;
            _url = url;
            _messageListener = listener;
            _clientFactory = clientFactory;
        }

        public async Task Send(DMessage message, bool flush = true)
        {
            if (!(message is InvokeMessage invokeMessage))
                return;
            var uri = new Uri(new Uri(_url), "micro/executor");
            var client = _clientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, uri.AbsoluteUri);
            if (invokeMessage.Headers != null)
            {
                foreach (var header in invokeMessage.Headers)
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }

            var data = await _codecFactory.GetEncoder().EncodeAsync(message);
            req.Content = new ByteArrayContent(data);
            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                throw new SpearException($"服务请求异常，状态码{(int)resp.StatusCode}");
            }
            var content = await resp.Content.ReadAsByteArrayAsync();
            var result = await _codecFactory.GetDecoder().DecodeAsync<MessageResult>(content);
            await _messageListener.OnReceived(this, result);

        }
    }
}
