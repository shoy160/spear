using Spear.ProxyGenerator.Proxy;
using System;

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

        public object Create(Type type, object key = null)
        {
            var instance = _resolver.Resolve(type, key);
            if (instance != null) return instance;
            var executor = new ProxyExecutor(_proxyProvider, key);
            instance = _proxyGenerator.CreateProxy(type, executor);
            _resolver.Register(type, instance, key);
            return instance;
        }
    }
}
