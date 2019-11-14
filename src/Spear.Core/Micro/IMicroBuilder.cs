using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Spear.Core.Micro
{
    public interface IMicroBuilder : IServiceCollection
    {
    }

    public interface IMicroClientBuilder : IMicroBuilder { }
    public interface IMicroServerBuilder : IMicroBuilder { }

    public class MicroBuilder : ServiceCollection, IMicroClientBuilder, IMicroServerBuilder
    {
    }

    public static class MicroBuilderExtentions
    {
        public static IMicroBuilder Register(this IMicroBuilder builder, Action<IServiceCollection> configServices)
        {
            configServices.Invoke(builder);
            return builder;
        }

        //public static IMicroBuilder AddSingleton<T, TImp>(this IMicroBuilder builder)
        //    where T : class
        //    where TImp : class, T
        //{
        //    builder.AddSingleton<T, TImp>();
        //    return builder;
        //}

        ///// <summary> 覆盖 单例 </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="TImp"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static IMicroBuilder TryAddSingleton<T, TImp>(this IMicroBuilder builder)
        //    where T : class
        //    where TImp : class, T
        //{
        //    builder.TryAddSingleton<T, TImp>();
        //    return builder;
        //}

        public static IMicroBuilder AddSingleton<T>(this IMicroBuilder builder, Func<IServiceProvider, T> implementationFunc)
        where T : class
        {
            builder.TryAddSingleton(implementationFunc);
            return builder;
        }
    }
}
