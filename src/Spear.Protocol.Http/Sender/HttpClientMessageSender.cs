using Newtonsoft.Json;
using Spear.Core.Message;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Spear.Protocol.Http.Sender
{
    public class HttpClientMessageSender : IMessageSender
    {
        private readonly IMessageDecoder _messageDecoder;
        private readonly IMessageListener _messageListener;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _url;

        public HttpClientMessageSender(IHttpClientFactory clientFactory, IMessageDecoder messageDecoder, string url, IMessageListener listener)
        {
            _messageDecoder = messageDecoder;
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

            req.Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var resp = await client.SendAsync(req);
            var content = await resp.Content.ReadAsByteArrayAsync();
            var result = _messageDecoder.Decode(content);
            await _messageListener.OnReceived(this, result);

        }
    }
}
