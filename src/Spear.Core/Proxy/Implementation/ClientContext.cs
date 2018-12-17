using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Spear.Core.Proxy
{
    public class ClientContext
    {
        internal static IServiceProvider Provider;

        public static object Resolve(Type type)
        {
            return Provider.GetService(type);
        }

        public static T Resolve<T>()
        {
            return Provider.GetService<T>();
        }

        public IEnumerable<T> Resolves<T>()
        {
            return Provider.GetServices<T>();
        }
    }
}
