using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spear.Core
{
    public class MicroServices
    {
        public static IServiceProvider Current { get; set; }
        public static T GetService<T>()
        {
            return Current.GetService<T>();
        }

        public static object GetService(Type type)
        {
            return Current.GetService(type);
        }
    }
}
