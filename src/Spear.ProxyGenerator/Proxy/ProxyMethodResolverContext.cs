using System.Reflection;

namespace Spear.ProxyGenerator.Proxy
{
    internal class ProxyMethodResolverContext
    {
        public PackedArgs Packed { get; }
        public MethodBase Method { get; }

        public ProxyMethodResolverContext(PackedArgs packed, MethodBase method)
        {
            Packed = packed;
            Method = method;
        }
    }
}
