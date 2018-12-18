using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator
{
    /// <summary> 代理提供者 </summary>
    public interface IProxyProvider
    {
        /// <summary> 执行代理 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="routePath"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, object key = null);
    }
}
