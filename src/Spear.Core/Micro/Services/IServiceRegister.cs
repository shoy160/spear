using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    /// <summary> 服务注册器 </summary>
    public interface IServiceRegister
    {
        /// <summary> 注册服务 </summary>
        Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress);

        /// <summary> 注销服务 </summary>
        Task Deregist();
    }
}
