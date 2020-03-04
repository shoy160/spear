using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator.Impl
{
    public class ProxyExecutor
    {
        private readonly IProxyProvider _proxyProvider;
        private readonly object _serviceKey;

        public ProxyExecutor(IProxyProvider provider, object serviceKey = null)
        {
            _proxyProvider = provider;
            _serviceKey = serviceKey;
        }

        private static IDictionary<string, object> GetParameters(MethodBase method, IReadOnlyList<object> args)
        {
            var dict = new Dictionary<string, object>();
            var parameters = method.GetParameters();
            if (!parameters.Any())
                return dict;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                dict.Add(parameter.Name, args[i]);
            }

            return dict;
        }

        public object Invoke(MethodInfo method, object[] args)
        {
            var parameters = GetParameters(method, args);
            return _proxyProvider.Invoke(method, parameters, _serviceKey);
        }

        public Task InvokeAsync(MethodInfo method, object[] args)
        {
            var parameters = GetParameters(method, args);
            return _proxyProvider.InvokeAsync(method, parameters, _serviceKey);
        }

        public Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            var parameters = GetParameters(method, args);
            return _proxyProvider.InvokeAsync<T>(method, parameters, _serviceKey);
        }
    }
}
