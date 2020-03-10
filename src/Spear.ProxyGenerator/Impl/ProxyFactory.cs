using System;
using Spear.ProxyGenerator.Proxy;

namespace Spear.ProxyGenerator.Impl
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IResolver _resolver;
        private readonly IProxyProvider _proxyProvider;
        private readonly AsyncProxyGenerator _proxyGenerator;

        public ProxyFactory(IResolver resolver, IProxyProvider proxyProvider, AsyncProxyGenerator proxyGenerator)
        {
            _resolver = resolver;
            _proxyProvider = proxyProvider;
            _proxyGenerator = proxyGenerator;
        }

        protected virtual object Resolve(Type type, object key = null)
        {
            return null;
        }

        public object Create(Type type, object key = null)
        {
            var instance = _resolver.Resolve(type, key);
            if (instance != null) return instance;
            instance = Resolve(type, key);
            if (instance != null) return instance;

            instance = _proxyGenerator.CreateProxy(type, typeof(ProxyExecutor), _proxyProvider, key);
            _resolver.Register(type, instance, key);
            return instance;
        }
    }
}
