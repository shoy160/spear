using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Spear.Core.Micro
{
    public interface IMicroBuilder
    {
        IServiceCollection Services { get; }
    }

    public interface IMicroClientBuilder : IMicroBuilder { }
    public interface IMicroServerBuilder : IMicroBuilder { }

    internal sealed class MicroBuilder : IMicroClientBuilder, IMicroServerBuilder
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

        /// <summary> 覆盖 单例 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImp"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMicroBuilder TryAddSingleton<T, TImp>(this IMicroBuilder builder)
            where T : class
            where TImp : class, T
        {
            builder.Services.TryAddSingleton<T, TImp>();
            return builder;
        }

        public static IMicroBuilder AddSingleton<T>(this IMicroBuilder builder, Func<IServiceProvider, T> implementationFunc)
        where T : class
        {
            builder.Services.TryAddSingleton(implementationFunc);
            return builder;
        }
    }
}
