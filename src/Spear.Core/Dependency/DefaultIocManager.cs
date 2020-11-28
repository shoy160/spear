using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spear.Core.Dependency
{
    public class DefaultIocManager : IIocManager
    {
        private readonly IServiceProvider _provider;

        public DefaultIocManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T Resolve<T>()
        {
            return _provider.GetService<T>();
        }

        public object Resolve(Type type)
        {
            return _provider.GetService(type);
        }

        public T Resolve<T>(string key)
        {
            return _provider.GetService<T>();
        }

        public object Resolve(string key, Type type)
        {
            return _provider.GetService(type);
        }

        public bool IsRegistered(Type type)
        {
            return _provider.GetService(type) != null;
        }

        public bool IsRegistered(string key, Type type)
        {
            return _provider.GetService(type) != null;
        }

        public bool IsRegistered<T>(string key)
        {
            return _provider.GetService<T>() != null;
        }

        public bool IsRegistered<T>()
        {
            return _provider.GetService<T>() != null;
        }

        public ILifetimeScope BeginLifetimeScope(object tag = null)
        {
            return new DefaultIocLifetimeScope(_provider);
        }
    }
}
