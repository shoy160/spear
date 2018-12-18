using System.Threading.Tasks;

namespace Spear.ProxyGenerator.Proxy
{
    public class ProxyHandler
    {
        private readonly AsyncProxyGenerator _generator;
        public ProxyHandler(AsyncProxyGenerator generator)
        {
            _generator = generator;
        }

        public object InvokeHandle(object[] args)
        {
            return _generator.Invoke(args);
        }

        public Task InvokeAsyncHandle(object[] args)
        {
            return _generator.InvokeAsync(args);
        }

        public Task<T> InvokeAsyncHandleT<T>(object[] args)
        {
            return _generator.InvokeAsync<T>(args);
        }
    }
}
