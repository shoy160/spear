using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Micro.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spear.Protocol.Http.Sender
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpClientMessageSender : IMessageSender
    {
        private readonly IServiceProvider _provider;
        private readonly ServiceAddress _address;
        private readonly IMessageListener _listener;
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientMessageSender(IServiceProvider provider, ServiceAddress address, IMessageListener listener)
        {
            _provider = provider;
            _address = address;
            _listener = listener;
            _clientFactory = provider.GetService<IHttpClientFactory>();
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

            var codec = _provider.GetClientCodec(_address.Codec);

            var data = await codec.EncodeAsync(message, _address.Gzip);
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
            var result = await codec.DecodeAsync<MessageResult>(content, _address.Gzip);
            await _listener.OnReceived(this, result);
        }
    }
}
