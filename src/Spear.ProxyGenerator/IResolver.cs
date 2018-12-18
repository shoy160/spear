using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spear.ProxyGenerator
{
    public interface IResolver
    {
        /// <summary> 注册 </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="key"></param>
        void Register(Type type, object value, object key = null);

        /// <summary> 获取服务 </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(Type type, object key = null);

        /// <summary> 获取该类型所有服务 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<object> Resolves(Type type);
    }

    public static class ResolverExtensions
    {
        /// <summary> 注册 </summary>
        /// <param name="resolver"></param>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public static void Register(this IResolver resolver, object value, object key = null)
        {
            resolver.Register(value.GetType(), value, key);
            var interFaces = value.GetType().GetTypeInfo().GetInterfaces();
            foreach (var interFace in interFaces)
            {
                resolver.Register(interFace, value, key);
            }
        }
    }
}
