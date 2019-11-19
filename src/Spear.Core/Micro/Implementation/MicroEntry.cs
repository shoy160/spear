using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    public class MicroEntry
    {
        /// <summary> 方法 </summary>
        public MethodInfo Method { get; }

        /// <summary> 执行代理 </summary>
        public Func<IDictionary<string, object>, Task<object>> Invoke { get; set; }

        /// <summary> 是否是Task </summary>
        public bool IsTask { get; }

        /// <summary> 是否是通知 </summary>
        public bool IsNotify { get; }

        /// <summary> 参数 </summary>
        public ParameterInfo[] Parameters { get; }

        /// <summary> Ctor </summary>
        /// <param name="method"></param>
        public MicroEntry(MethodInfo method)
        {
            Method = method;
            IsTask = Method.ReturnType == typeof(Task) || Method.ReturnType == typeof(Task<>);
            IsNotify = Method.ReturnType == typeof(void) || Method.ReturnType == typeof(Task);
            Parameters = method.GetParameters();
        }
    }
}
