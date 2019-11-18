using System.ComponentModel.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests.Service
{
    public class InstanceListRequest
    {
        /// <summary> 命名空间ID </summary>
        public string namespaceId { get; set; }

        /// <summary> 集群名,多个集群用逗号分隔 </summary>
        public string clusters { get; set; }

        /// <summary> 分组名 </summary>
        public string groupName { get; set; }

        /// <summary> 服务名 </summary>
        [Required(ErrorMessage = "请输入服务名称")]
        public string serviceName { get; set; }

        /// <summary> 是否只返回健康实例 </summary>
        public bool healthyOnly { get; set; } = true;
    }
}
