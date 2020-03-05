using Spear.Core.Message.Implementation;
using Spear.Core.Message.Json.Models;

namespace Spear.Core.Message.Json
{
    public class JsonCodec : DMessageCodec<JsonDynamic, JsonInvokeMessage, JsonResultMessage>
    {
        public JsonCodec(IMessageSerializer serializer) : base(serializer)
        {
        }
    }
}
