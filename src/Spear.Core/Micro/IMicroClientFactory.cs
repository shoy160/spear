using System.Threading.Tasks;
using Spear.Core.Micro.Services;

namespace Spear.Core.Micro
{
    /// <summary> Spear客户端工厂 </summary>
    public interface IMicroClientFactory
    {
        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress"></param>
        /// <returns></returns>
        Task<IMicroClient> CreateClient(ServiceAddress serviceAddress);
    }
}
