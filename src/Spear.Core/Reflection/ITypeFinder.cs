using System;

namespace Spear.Core.Reflection
{
    /// <summary> 类型查找器 </summary>
    public interface ITypeFinder
    {
        Type[] Find(Func<Type, bool> typeFunc = null);
    }
}
