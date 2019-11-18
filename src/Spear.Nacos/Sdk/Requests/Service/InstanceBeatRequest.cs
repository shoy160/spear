using System.ComponentModel.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests.Service
{
    public class InstanceBeatRequest
    {
        /// <summary> 命名空间ID </summary>
        public string namespaceId { get; set; }

        /// <summary> 分组名 </summary>
        public string groupName { get; set; }

        /// <summary> 服务名 </summary>
        [Required(ErrorMessage = "请输入服务名称")]
        public string serviceName { get; set; }

        /// <summary> 是否临时实例 </summary>
        public bool ephemeral { get; set; }

        /// <summary> 实例心跳内容 </summary>
        [Required(ErrorMessage = "请输入心跳内容")]
        public string beat { get; set; }
    }
}
