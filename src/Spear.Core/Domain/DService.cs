using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using System;

namespace Spear.Core.Domain
{
    /// <summary> 基础服务类 </summary>
    public abstract class DService
    {
        /// <summary> 日志服务 </summary>
        protected ILogger Logger
        {
            get
            {
                return CurrentIocManager.Resolve<ILoggerFactory>()?.CreateLogger(GetType());
            }
        }

        /// <summary> 操作单元 </summary>
        protected IUnitOfWork UnitOfWork => Resolve<IUnitOfWork>();

        /// <summary> 获取IOC注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Resolve<T>()
        {
            return CurrentIocManager.Resolve<T>();
        }

        /// <summary> 获取IOC注入 </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        protected object Resolve(Type interfaceType)
        {
            return CurrentIocManager.Resolve(interfaceType);
        }

        /// <summary> 获取IOC注入 </summary>
        /// <param name="key"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        protected object Resolve(string key, Type interfaceType)
        {
            return CurrentIocManager.Resolve(key, interfaceType);
        }
    }
}
