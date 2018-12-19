using Acb.Core.Helper.Http;
using Spear.Core.Message;
using System;
using System.Threading.Tasks;

namespace Spear.Protocol.Http.Sender
{
    public class HttpClientMessageSender : IMessageSender
    {
        private readonly IMessageDecoder _messageDecoder;
        private readonly IMessageListener _messageListener;
        private readonly string _url;

        public HttpClientMessageSender(IMessageDecoder messageDecoder, string url, IMessageListener listener)
        {
            _messageDecoder = messageDecoder;
            _url = url;
            _messageListener = listener;
        }

        public async Task Send(MicroMessage message, bool flush = true)
        {
            if (!message.IsInvoke)
                return;
            var invoke = message.GetContent<InvokeMessage>();
            var uri = new Uri(new Uri(_url), "micro");
            var resp = await HttpHelper.Instance.PostAsync(new HttpRequest(uri.AbsoluteUri)
            {
                Headers = invoke.Headers,
                Data = message
            });
            var content = await resp.Content.ReadAsByteArrayAsync();
            var result = _messageDecoder.Decode(content);
            await _messageListener.OnReceived(this, result);

        }
    }
}
