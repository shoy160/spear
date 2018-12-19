using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator
{
    /// <summary> 代理提供者 </summary>
    public interface IProxyProvider
    {
        /// <summary> 执行代理 </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Invoke(MethodInfo method, IDictionary<string, object> parameters, object key = null);

        /// <summary> 执行代理 </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task InvokeAsync(MethodInfo method, IDictionary<string, object> parameters, object key = null);

        /// <summary> 执行代理 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> InvokeAsync<T>(MethodInfo method, IDictionary<string, object> parameters, object key = null);
    }
}
