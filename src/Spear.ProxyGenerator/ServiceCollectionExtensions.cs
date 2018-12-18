using Microsoft.Extensions.DependencyInjection;
using Spear.ProxyGenerator.Impl;
using Spear.ProxyGenerator.Proxy;

namespace Spear.ProxyGenerator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy(this IServiceCollection services)
        {
            services.AddSingleton<AsyncProxyGenerator>();
            services.AddSingleton<IResolver, ProxyResolver>();
            services.AddSingleton<IProxyProvider, ProxyProvider>();
            services.AddSingleton<IProxyFactory, ProxyFactory>();
            return services;
        }
    }
}
