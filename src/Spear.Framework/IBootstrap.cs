using System;

namespace Spear.Framework
{
    /// <summary> 启动类接口 </summary>
    public interface IBootstrap : IDisposable
    {
        /// <summary> 初始化 </summary>
        void Initialize();
    }
}
