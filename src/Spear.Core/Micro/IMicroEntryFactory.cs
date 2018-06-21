using System.Reflection;
using Acb.Core.Dependency;

namespace Spear.Core.Micro
{
    public interface IMicroEntryFactory : ISingleDependency
    {
        /// <summary> 获取服务条码Id </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        string GetServiceId(MethodInfo method);
        /// <summary> 查找服务条目 </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        MethodInfo Find(string serviceId);
    }
}
