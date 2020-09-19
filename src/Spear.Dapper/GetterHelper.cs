using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Spear.Dapper
{
    public class GetterHelper<T> : IDisposable
    {
        private static readonly IDictionary<string, Func<T, object>> GettersCache =
            new Dictionary<string, Func<T, object>>();

        private static Func<T, object> GetValueGetter(PropertyInfo propertyInfo)
        {
            var targetType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetGetMethod();

            var exTarget = Expression.Parameter(targetType ?? throw new InvalidOperationException(), "t");
            var exBody = Expression.Call(exTarget, methodInfo);
            var exBody2 = Expression.Convert(exBody, typeof(object));

            var lambda = Expression.Lambda<Func<T, object>>(exBody2, exTarget);

            var action = lambda.Compile();
            return action;
        }

        public void AddGetter(PropertyInfo prop)
        {
            if (GettersCache.ContainsKey(prop.Name))
                return;
            GettersCache.Add(prop.Name, GetValueGetter(prop));
        }

        public object GetValue(T instance, PropertyInfo prop)
        {
            if (GettersCache.TryGetValue(prop.Name, out var getter))
                return getter(instance);
            getter = GetValueGetter(prop);
            GettersCache.Add(prop.Name, getter);
            return getter(instance);
        }

        public void Dispose()
        {
            GettersCache?.Clear();
        }
    }
}
