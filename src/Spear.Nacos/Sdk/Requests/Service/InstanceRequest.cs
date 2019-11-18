using System.ComponentModel.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests.Service
{
    public class InstanceRequest
    {
        /// <summary> 命名空间ID </summary>
        public string namespaceId { get; set; }

        /// <summary> 集群名 </summary>
        public string clusterName { get; set; }

        /// <summary> 分组名 </summary>
        public string groupName { get; set; }

        /// <summary> 服务名 </summary>
        [Required(ErrorMessage = "请输入服务名称")]
        public string serviceName { get; set; }

        /// <summary> 服务实例IP </summary>
        [Required(ErrorMessage = "请输入服务IP")]
        public string ip { get; set; }

        /// <summary> 服务实例port </summary>
        public int port { get; set; }
    }
}
