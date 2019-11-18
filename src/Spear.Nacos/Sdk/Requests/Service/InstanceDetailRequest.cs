namespace Spear.Nacos.Sdk.Requests.Service
{
    public class InstanceDetailRequest : InstanceRequest
    {
        /// <summary> 是否只返回健康实例 </summary>
        public bool healthyOnly { get; set; } = true;

        /// <summary> 是否临时实例 </summary>
        public bool ephemeral { get; set; }
    }
}
