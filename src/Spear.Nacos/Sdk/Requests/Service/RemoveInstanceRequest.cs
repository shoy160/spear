namespace Spear.Nacos.Sdk.Requests.Service
{
    public class RemoveInstanceRequest : InstanceRequest
    {
        /// <summary> 是否临时实例 </summary>
        public bool ephemeral { get; set; }
    }
}
