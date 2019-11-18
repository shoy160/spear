namespace Spear.Nacos.Sdk.Requests.Service
{
    public class EditInstanceRequest : InstanceRequest
    {
        /// <summary> 扩展信息 </summary>
        public string metadata { get; set; }

        /// <summary> 权重 </summary>
        public double weight { get; set; }

        /// <summary> 是否打开流量 </summary>
        public bool enabled { get; set; } = true;

        /// <summary> 是否临时实例 </summary>
        public bool ephemeral { get; set; }
    }
}
