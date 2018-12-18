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

        public object Invoke(MethodInfo method, object[] args)
        {
            return InvokeAsyncT<object>(method, args);
        }

        public Task InvokeAsync(MethodInfo method, object[] args)
        {
            return InvokeAsyncT<object>(method, args);
        }

        public Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            var methodType = method.DeclaringType;
            var serviceId = $"{methodType?.FullName}.{method.Name}";
            var dict = new Dictionary<string, object>();
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    dict.Add(parameters[i].Name, args[i]);
                }
                serviceId += "_" + string.Join("_", parameters.Select(i => i.Name));
            }

            return _proxyProvider.Invoke<T>(dict, serviceId, _serviceKey);
        }
    }
}
