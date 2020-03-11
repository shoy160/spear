using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Message.Models;

namespace Spear.Protocol.Http.Sender
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpServerMessageSender : IMessageSender
    {
        private readonly IMessageEncoder _encoder;
        private readonly HttpResponse _response;
        private readonly bool _gzip;

        public HttpServerMessageSender(IMessageEncoder encoder, HttpResponse response, bool gzip)
        {
            _encoder = encoder;
            _response = response;
            _gzip = gzip;
        }

        public async Task Send(DMessage message, bool flush = true)
        {
            if (!(message is MessageResult result))
                return;
            var data = await _encoder.EncodeAsync(result, _gzip);
            var contentLength = data.Length;
            _response.Headers.Add("Content-Type", "application/json");
            _response.Headers.Add("Content-Length", contentLength.ToString());
            await _response.Body.WriteAsync(data, 0, data.Length);
            if (flush)
                await _response.Body.FlushAsync();
        }
    }
}
