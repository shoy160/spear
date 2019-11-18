using Newtonsoft.Json;
using System.Collections.Generic;

namespace Spear.Nacos.Sdk.Requests.Service
{
    public class CreateInstanceRequest : InstanceRequest
    {
        public IDictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();

        /// <summary> 扩展信息 </summary>
        public string metadata => JsonConvert.SerializeObject(Meta);

        /// <summary> 权重 </summary>
        public double weight { get; set; }

        /// <summary> 是否上线 </summary>
        public bool enabled { get; set; } = true;

        /// <summary> 是否健康 </summary>
        public bool healthy { get; set; } = true;

        /// <summary> 是否临时实例 </summary>
        public bool ephemeral { get; set; }

    }
}
