using Spear.Core.Dependency;

namespace Spear.Framework
{
    /// <summary> 启动基类 </summary>
    public abstract class AbstractBootstrap : IBootstrap
    {
        /// <summary> 初始化 </summary>
        public abstract void Initialize();

        /// <summary> 是否已释放 </summary>
        protected bool IsDisposed;

        /// <summary> 注入管理 </summary>
        public IIocManager IocManager { get; protected set; }

        /// <summary> 注册依赖 </summary>
        protected abstract void IocRegisters();

        ///// <summary> 初始化各个模块 </summary>
        //protected void ModulesInstaller()
        //{
        //    IocManager?.Resolve<IModuleManager>().InitializeModules();
        //}

        /// <summary> 缓存初始化 </summary>
        protected abstract void CacheInit();

        /// <summary> 日志初始化 </summary>
        protected abstract void LoggerInit();

        /// <summary> 数据库初始化 </summary>
        protected abstract void DatabaseInit();

        /// <summary> 释放资源 </summary>
        public virtual void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            //IocManager.Resolve<IModuleManager>().ShutdownModules();
        }
    }
}
