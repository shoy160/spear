using System;

namespace Spear.Core.Dependency
{
    /// <summary> 依赖注入管理器 </summary>
    public interface IIocManager
    {
        /// <summary> 获取注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();

        /// <summary> 获取注入 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary> 获取注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>(string key);

        /// <summary> 获取注入 </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(string key, Type type);

        /// <summary> 是否注册注入 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(Type type);

        /// <summary> 是否注册注入 </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(string key, Type type);

        /// <summary> 是否注册注入 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRegistered<T>(string key);

        /// <summary> 是否注册注入 </summary>
        /// <returns></returns>
        bool IsRegistered<T>();

        /// <summary> 启动生命周期 </summary>
        /// <returns></returns>
        ILifetimeScope BeginLifetimeScope(object tag = null);
    }

    /// <summary> 生命周期IOC容器 </summary>
    public interface ILifetimeScope : IIocManager, IDisposable
    {
    }
}
