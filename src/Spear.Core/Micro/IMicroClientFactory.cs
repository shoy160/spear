using Spear.Core.Micro.Services;

namespace Spear.Core.Micro
{
    public interface IMicroClientFactory
    {
        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        IMicroClient CreateClient(ServiceAddress serviceAddress);
    }
}
