namespace Spear.Core.Proxy
{
    public interface IClientProxy
    {
        T Create<T>(string name);
    }

    public static class ClientProxyExtensions
    {
        public static T Create<T>(this IClientProxy proxy)
        {
            return proxy.Create<T>(string.Empty);
        }
    }
}
