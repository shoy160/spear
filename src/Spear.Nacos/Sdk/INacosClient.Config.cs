using Spear.Nacos.Sdk.Filters;
using Spear.Nacos.Sdk.Requests;
using WebApiClient;
using WebApiClient.Attributes;

namespace Spear.Nacos.Sdk
{
    public partial interface INacosClient
    {
        /// <summary> 获取配置 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("/nacos/v1/cs/configs"), ConfigFilter]
        ITask<string> GetConfigAsync([PathQuery]GetConfigRequest request);

        /// <summary> 发布配置 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/nacos/v1/cs/configs"), ConfigFilter]
        ITask<bool> PublishConfigAsync([FormContent]PublishConfigRequest request);

        /// <summary> 删除配置 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("/nacos/v1/cs/configs"), ConfigFilter]
        ITask<bool> RemoveConfigAsync(RemoveConfigRequest request);

        /// <summary> 添加监控 </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpPost("/nacos/v1/cs/configs/listener"), ConfigFilter]
        ITask<string> AddListenerAsync([FormContent]AddListenerRequest request, [Header("Long-Pulling-Timeout")] long timeout = 30000);

        ///// <summary> 删除监控 </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //ITask<string> RemoveListenerAsync(RemoveListenerRequest request);
    }
}
