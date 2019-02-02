using Microsoft.AspNetCore.Http;
using Spear.Core.Message;
using System.Threading.Tasks;
using Spear.Core;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.Http.Sender
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpServerMessageSender : IMessageSender
    {
        private readonly IMessageEncoder _encoder;
        private readonly HttpResponse _response;

        public HttpServerMessageSender(IMessageEncoder encoder, HttpResponse response)
        {
            _encoder = encoder;
            _response = response;
        }

        public async Task Send(MicroMessage message, bool flush = true)
        {
            if (!message.IsResult)
                return;
            var data = _encoder.Encode(message);
            var contentLength = data.Length;
            _response.Headers.Add("Content-Type", "application/json");
            _response.Headers.Add("Content-Length", contentLength.ToString());
            await _response.Body.WriteAsync(data, 0, data.Length);
            if (flush)
                await _response.Body.FlushAsync();
        }
    }
}
