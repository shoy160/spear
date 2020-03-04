using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    /// <summary> 服务探测器 </summary>
    public interface IServiceFinder
    {
        /// <summary> 服务发现 </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        Task<List<ServiceAddress>> Find(Type serviceType);

        /// <summary> 清除服务缓存 </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        Task CleanCache(Type serviceType);
    }
}
