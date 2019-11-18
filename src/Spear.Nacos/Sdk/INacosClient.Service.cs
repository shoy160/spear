using Spear.Nacos.Sdk.Requests.Service;
using Spear.Nacos.Sdk.Results;
using WebApiClient;
using WebApiClient.Attributes;

namespace Spear.Nacos.Sdk
{
    public partial interface INacosClient
    {
        /// <summary> 注册实例 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/nacos/v1/ns/instance")]
        ITask<string> CreateInstance(CreateInstanceRequest request);

        /// <summary> 修改实例 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("/nacos/v1/ns/instance")]
        ITask<string> EditInstance(EditInstanceRequest request);

        /// <summary> 注销实例 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("/nacos/v1/ns/instance")]
        ITask<string> RemoveInstance(RemoveInstanceRequest request);

        /// <summary> 查询实例列表 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("/nacos/v1/ns/instance/list")]
        ITask<ListInstanceResult> InstanceList(InstanceListRequest request);

        /// <summary> 查询实例详情 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("/nacos/v1/ns/instance")]
        ITask<InstanceDetailResult> InstanceDetail(InstanceDetailRequest request);

        /// <summary> 发送实例心跳 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("/nacos/v1/ns/instance/beat")]
        ITask<string> InstanceBeat([PathQuery]InstanceBeatRequest request);
    }
}
