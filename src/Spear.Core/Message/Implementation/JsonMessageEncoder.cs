using Newtonsoft.Json;
using System.Text;

namespace Spear.Core.Message.Implementation
{
    public class JsonMessageEncoder : IMessageEncoder
    {
        public byte[] Encode(MicroMessage message)
        {
            var content = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(content);
        }
    }
}
