using Spear.ProxyGenerator.Proxy;

namespace Spear.ProxyGenerator.Impl
{
    public class TestService : ProxyExecutor
    {
        private readonly ProxyHandler _handler;
        public TestService(IProxyProvider provider, object serviceKey, ProxyHandler handler) : base(provider, serviceKey)
        {
            _handler = handler;
        }
    }
}
