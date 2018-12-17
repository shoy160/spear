using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    /// <summary> 服务探测器 </summary>
    public interface IServiceFinder
    {
        Task<List<ServiceAddress>> Find(Type serviceType);
    }
}
