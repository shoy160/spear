using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator.Impl
{
    public class ProxyProvider : IProxyProvider
    {
        public Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, object key = null)
        {
            throw new NotImplementedException();
        }
    }
}
