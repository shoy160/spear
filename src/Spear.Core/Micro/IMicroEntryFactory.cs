using System;
using System.Collections.Generic;
using System.Reflection;
using Spear.Core.Micro.Implementation;

namespace Spear.Core.Micro
{
    /// <summary> 服务发现工厂 </summary>
    public interface IMicroEntryFactory
    {
        /// <summary> 方法列表 </summary>
        IDictionary<string, MicroEntry> Entries { get; }

        List<Type> Services { get; }

        /// <summary> 获取所有服务程序集 </summary>
        /// <returns></returns>
        IEnumerable<Assembly> GetContracts();

        /// <summary> 获取服务条码Id </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        string GetServiceId(MethodInfo method);

        /// <summary> 查找服务条目 </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        MicroEntry Find(string serviceId);
    }
}
