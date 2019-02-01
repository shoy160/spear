using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spear.Core.Reflection
{
    /// <summary> 程序集查找器 </summary>
    public interface IAssemblyFinder
    {
        IEnumerable<Assembly> Find(Func<Assembly, bool> assemblyFunc = null);
    }
}
