using Newtonsoft.Json;
using Spear.Core.Message.Models;

namespace Spear.Core.Message.Json.Models
{
    [JsonObject]
    public class JsonDynamic : DMessageDynamic
    {
        [JsonProperty(Order = 1)]
        public override string ContentType { get; set; }

        [JsonProperty(Order = 2)]
        public override byte[] Content { get; set; }

        public JsonDynamic() : base(new JsonMessageSerializer()) { }
    }
}
