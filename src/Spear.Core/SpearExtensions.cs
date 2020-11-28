using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spear.Core
{
    public static class SpearExtensions
    {
        /// <summary> 服务命名 </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string ServiceName(this Assembly assembly)
        {
            var assName = assembly.GetName();
            return $"{assName.Name}_v{assName.Version.Major}";
        }

        public static string TypeName(this Type type)
        {
            var code = Type.GetTypeCode(type);
            if (code != TypeCode.Object && type.BaseType != typeof(Enum))
                return type.FullName;
            return type.AssemblyQualifiedName;
        }

        private static readonly IDictionary<MethodInfo, string> RouteCache = new Dictionary<MethodInfo, string>();

        /// <summary> 获取服务主键 </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string ServiceKey(this MethodInfo method)
        {
            if (RouteCache.TryGetValue(method, out var route))
                return route;
            var key = string.Empty;
            var attr = method.DeclaringType?.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null)
                key = attr.Route;
            attr = method.GetCustomAttribute<ServiceRouteAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Route))
                route = (attr.Route.StartsWith("/") ? attr.Route.TrimStart('/') : $"{key}/{attr.Route}").ToLower();
            else if (!string.IsNullOrWhiteSpace(key))
            {
                route = $"{key}/{method.Name}".ToLower();
            }
            else
            {
                route = $"{method.DeclaringType?.Name}/{method.Name}".ToLower();
            }
            RouteCache.Add(method, route);

            return route;
        }
    }
}
