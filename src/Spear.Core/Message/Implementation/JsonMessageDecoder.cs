using Newtonsoft.Json;
using System.Text;

namespace Spear.Core.Message.Implementation
{
    public class JsonMessageDecoder : IMessageDecoder
    {
        public MicroMessage Decode(byte[] data)
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
