using Microsoft.Extensions.DependencyInjection;
using Spear.ProxyGenerator.Impl;
using Spear.ProxyGenerator.Proxy;

namespace Spear.ProxyGenerator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy<T>(this IServiceCollection services)
        where T : class, IProxyProvider
        {
            services.AddSingleton<AsyncProxyGenerator>();
            services.AddSingleton<IResolver, ProxyResolver>();
            services.AddSingleton<IProxyProvider, T>();
            services.AddSingleton<IProxyFactory, ProxyFactory>();
            return services;
        }
    }
}
