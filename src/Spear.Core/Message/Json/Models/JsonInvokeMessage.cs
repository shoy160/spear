using System.Collections.Generic;
using Newtonsoft.Json;
using Spear.Core.Message.Models;

namespace Spear.Core.Message.Json.Models
{
    [JsonObject]
    public class JsonInvokeMessage : DMessageInvoke<JsonDynamic>
    {
        [JsonProperty(Order = 1)]
        public override string Id { get; set; }

        /// <summary> 服务Id </summary>
        [JsonProperty(Order = 2)]
        public override string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        [JsonProperty(Order = 3)]
        public override bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        [JsonProperty(Order = 4)]
        public override IDictionary<string, JsonDynamic> Parameters { get; set; }

        [JsonProperty(Order = 5)]
        public override IDictionary<string, string> Headers { get; set; }

        public JsonInvokeMessage() { }
        public JsonInvokeMessage(InvokeMessage message) : base(message)
        {
        }
    }
}
