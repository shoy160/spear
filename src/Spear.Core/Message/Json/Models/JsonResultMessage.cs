using Newtonsoft.Json;
using Spear.Core.Message.Models;

namespace Spear.Core.Message.Json.Models
{
    [JsonObject]
    public class JsonResultMessage : DMessageResult<JsonDynamic>
    {
        [JsonProperty(Order = 1)]
        public override string Id { get; set; }

        /// <summary> 状态码 </summary>
        [JsonProperty(Order = 2)]
        public override int Code { get; set; }

        /// <summary> 错误消息 </summary>
        [JsonProperty(Order = 3)]
        public override string Message { get; set; }

        /// <summary> 数据实体 </summary>
        [JsonProperty(Order = 4)]
        public override JsonDynamic Content { get; set; }

        public JsonResultMessage() { }

        public JsonResultMessage(MessageResult message) : base(message)
        {
        }
    }
}
