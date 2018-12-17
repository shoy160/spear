using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spear.Core.Micro
{
    public interface IMicroBuilder
    {
        IServiceCollection Services { get; }
    }

    internal sealed class MicroBuilder : IMicroBuilder
    {
        public MicroBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public IServiceCollection Services { get; }
    }

    public static class MicroBuilderExtentions
    {
        public static IMicroBuilder Register(this IMicroBuilder builder, Action<IServiceCollection> configServices)
        {
            configServices.Invoke(builder.Services);
            return builder;
        }

        public static IMicroBuilder AddSingleton<T, TImp>(this IMicroBuilder builder)
            where T : class
            where TImp : class, T
        {
            builder.Services.AddSingleton<T, TImp>();
            return builder;
        }

        public static IMicroBuilder AddSingleton<T>(this IMicroBuilder builder, Func<IServiceProvider, T> implementationFunc)
        where T : class
        {
            builder.Services.AddSingleton(implementationFunc);
            return builder;
        }
    }
}
