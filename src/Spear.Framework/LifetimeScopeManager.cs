using Autofac;
using Spear.Core.Dependency;
using System;
using ILifetimeScope = Spear.Core.Dependency.ILifetimeScope;

namespace Spear.Framework
{
    public class LifetimeScopeManager : ILifetimeScope
    {
        private readonly Autofac.ILifetimeScope _scope;

        public LifetimeScopeManager(object tag = null)
        {
            var container = CurrentIocManager.Resolve<IocManager>().Current;
            _scope = tag == null ? container.BeginLifetimeScope() : container.BeginLifetimeScope(tag);
        }

        /// <summary> 获取Ioc注入实例 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return _scope.Resolve<T>();
        }

        /// <summary> 获取Ioc注入实例 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return _scope.Resolve(type);
        }

        public T Resolve<T>(string key)
        {
            return _scope.ResolveKeyed<T>(key);
        }

        public object Resolve(string key, Type type)
        {
            return _scope.ResolveKeyed(key, type);
        }

        /// <summary> 是否注册Ioc注入 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsRegistered(Type type)
        {
            return _scope.IsRegistered(type);
        }

        public bool IsRegistered(string key, Type type)
        {
            return _scope.IsRegisteredWithKey(key, type);
        }

        public bool IsRegistered<T>(string key)
        {
            return _scope.IsRegisteredWithKey<T>(key);
        }

        public bool IsRegistered<T>()
        {
            return _scope.IsRegistered<T>();
        }

        public ILifetimeScope BeginLifetimeScope(object tag = null)
        {
            return new LifetimeScopeManager(tag);
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}
