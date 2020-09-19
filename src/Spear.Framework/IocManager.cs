using Autofac;
using Spear.Core.Dependency;
using System;

namespace Spear.Framework
{
    public class IocManager : IIocManager
    {
        private readonly SpearBootstrap _bootstrap;

        internal Autofac.ILifetimeScope Current => _bootstrap.ContainerRoot;

        public IocManager(SpearBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
        }

        public void MapService(Action<ContainerBuilder> buidlerAction)
        {
            _bootstrap.ReBuild(buidlerAction);
        }

        /// <summary> 获取Ioc注入实例 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            //if (AcbHttpContext.Current?.RequestServices != null)
            //    return AcbHttpContext.Current.RequestServices.GetService<T>();
            return Current.Resolve<T>();
        }

        /// <summary> 获取Ioc注入实例 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            //if (AcbHttpContext.Current?.RequestServices != null)
            //    return AcbHttpContext.Current.RequestServices.GetService(type);
            return Current.Resolve(type);
        }

        public T Resolve<T>(string key)
        {
            //if (AcbHttpContext.Current?.RequestServices != null)
            //    return AcbHttpContext.Current.RequestServices.GetService<T>(key);
            return Current.ResolveKeyed<T>(key);
        }

        public object Resolve(string key, Type type)
        {
            //if (AcbHttpContext.Current?.RequestServices != null)
            //    return AcbHttpContext.Current.RequestServices.GetService(type, key);
            return Current.ResolveKeyed(key, type);
        }

        /// <summary> 是否注册Ioc注入 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsRegistered(Type type)
        {
            return Current.IsRegistered(type);
        }

        public bool IsRegistered(string key, Type type)
        {
            return Current.IsRegisteredWithKey(key, type);
        }

        public bool IsRegistered<T>(string key)
        {
            return Current.IsRegisteredWithKey<T>(key);
        }

        public bool IsRegistered<T>()
        {
            return Current.IsRegistered<T>();
        }

        public Core.Dependency.ILifetimeScope BeginLifetimeScope(object tag = null)
        {
            return new LifetimeScopeManager(tag);
        }
    }
}
