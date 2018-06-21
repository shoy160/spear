using System.Text;
using Newtonsoft.Json;

namespace Spear.Core.Message
{
    public class JsonMessageCoderFactory : IMessageCoderFactory
    {
        private readonly IMessageEncoder _encoder = new JsonMessageEncoder();
        private readonly IMessageDecoder _decoder = new JsonMessageDecoder();
        public IMessageEncoder GetEncoder()
        {
            return _encoder;
        }

        public IMessageDecoder GetDecoder()
        {
            return _decoder;
        }
    }

    public class JsonMessageEncoder : IMessageEncoder
    {
        public byte[] Encode(IMicroMessage message)
        {
            var content = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(content);
        }
    }

    public class JsonMessageDecoder : IMessageDecoder
    {
        public IMicroMessage Decode(byte[] data)
        {
            var content = Encoding.UTF8.GetString(data);
            var message = JsonConvert.DeserializeObject<MicroMessage>(content);
            if (message.IsInvoke)
            {
                message.Content = JsonConvert.DeserializeObject<InvokeMessage>(message.Content.ToString());
            }
            if (message.IsResult)
            {
                message.Content = JsonConvert.DeserializeObject<ResultMessage>(message.Content.ToString());
            }
            return message;
        }
    }
}
