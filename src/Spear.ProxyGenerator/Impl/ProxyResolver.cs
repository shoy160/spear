using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Spear.ProxyGenerator.Impl
{
    /// <summary> 代理解析器 </summary>
    internal class ProxyResolver : IResolver
    {
        private readonly ConcurrentDictionary<Type, IDictionary<object, object>> _initializers =
            new ConcurrentDictionary<Type, IDictionary<object, object>>();

        private static object ConvertKey(object key)
        {
            return key ?? "{{NULL}}";
        }

        public virtual void Register(Type type, object value, object key = null)
        {
            key = ConvertKey(key);
            if (_initializers.ContainsKey(type))
            {
                if (_initializers.TryGetValue(type, out var dict))
                    dict[key] = value;
            }
            else
            {
                _initializers.TryAdd(type, new Dictionary<object, object> { { key, value } });
            }
        }

        public virtual object Resolve(Type type, object key = null)
        {
            key = ConvertKey(key);
            if (_initializers.TryGetValue(type, out var result) && result.TryGetValue(key, out var service))
            {
                return service;
            }

            return null;
        }

        public virtual IEnumerable<object> Resolves(Type type)
        {
            return _initializers.TryGetValue(type, out var result) ? result.Values : null;
        }
    }
}
