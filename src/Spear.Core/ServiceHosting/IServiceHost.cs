using System;

namespace Spear.Core.ServiceHosting
{
    /// <summary> 服务主机 </summary>
    public interface IServiceHost : IDisposable
    {
        /// <summary> 运行服务 </summary>
        /// <returns></returns>
        IDisposable Run();

        /// <summary> 初始化服务主机 </summary>
        /// <returns></returns>
        void Initialize();
    }
}
