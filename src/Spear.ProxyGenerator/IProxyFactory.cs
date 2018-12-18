using System;

namespace Spear.ProxyGenerator
{
    /// <summary> 代理工厂 </summary>
    public interface IProxyFactory
    {
        /// <summary> 创建代理 </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Create(Type type, object key = null);
    }

    public static class ProxyFactoryExtensions
    {
        public static T Create<T>(this IProxyFactory proxyFactory, object key = null)
            where T : class
        {
            return (T)proxyFactory.Create(typeof(T), key);
        }
    }
}
