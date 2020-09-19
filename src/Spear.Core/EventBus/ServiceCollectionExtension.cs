using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Spear.Core.EventBus
{
    public static class ServiceCollectionExtension
    {
        public static IEventBus GetEventBus(this IServiceProvider provider, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return provider.GetService<IEventBus>();
            var list = provider.GetServices<IEventBus>();
            if (list == null) return null;
            var busses = list as IEventBus[] ?? list.ToArray();
            return busses.FirstOrDefault(t => t.Name == name) ??
                   busses.FirstOrDefault(t => string.IsNullOrWhiteSpace(t.Name));
        }

        /// <summary> 开启订阅 </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceProvider SubscribeAt(this IServiceProvider provider)
        {
            provider.GetService<ISubscribeAdapter>().SubscribeAt();
            return provider;
        }
    }
}
