using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Spear.Core.Context;
using Spear.Core.Extensions;

namespace Spear.Core.Dependency
{

    public static class CurrentIocManager
    {
        /// <summary> 依赖注入管理器 </summary>
        public static IIocManager IocManager { get; private set; }

        public static void SetIocManager(IIocManager manager)
        {
            if (IocManager != null) return;
            IocManager = manager;
        }

        /// <summary> 获取注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return !IsRegisted<T>() ? default : IocManager.Resolve<T>();
        }

        /// <summary> 获取注入 </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static object Resolve(Type interfaceType)
        {
            return !IsRegisted(interfaceType) ? null : IocManager.Resolve(interfaceType);
        }

        /// <summary> 获取注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>(string key)
        {
            return !IsRegisted<T>(key) ? default : IocManager.Resolve<T>(key);
        }

        /// <summary> 获取注入 </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Resolve(string key, Type type)
        {
            return !IsRegisted(key, type) ? null : IocManager.Resolve(key, type);
        }

        /// <summary> 是否已注册 </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsRegisted(Type interfaceType)
        {
            return IocManager != null && IocManager.IsRegistered(interfaceType);
        }

        /// <summary> 是否注册注入 </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRegisted(string key, Type type)
        {
            return IocManager != null && IocManager.IsRegistered(key, type);
        }

        /// <summary> 是否注册注入 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsRegisted<T>(string key)
        {
            return IocManager != null && IocManager.IsRegistered<T>(key);
        }

        /// <summary> 是否注册注入 </summary>
        /// <returns></returns>
        public static bool IsRegisted<T>()
        {
            return IocManager != null && IocManager.IsRegistered<T>();
        }

        /// <summary> 开启生命周期 </summary>
        /// <returns></returns>
        public static ILifetimeScope BeginLifetimeScope()
        {
            return IocManager.BeginLifetimeScope();
        }

        public static ILogger CreateLogger<T>()
        {
            return Resolve<ILoggerFactory>()?.CreateLogger<T>();
        }

        public static ILogger CreateLogger(Type type)
        {
            return Resolve<ILoggerFactory>()?.CreateLogger(type);
        }

        /// <summary> 当前请求上下文 </summary>
        public static HttpContext Context => Resolve<IHttpContextAccessor>()?.HttpContext;

        /// <summary> 当前请求上下文封装 </summary>
        public static HttpContextWrap ContextWrap => Context?.Wrap();
    }
}
