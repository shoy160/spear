using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Spear.Core.Extensions
{
    /// <summary> IOC服务扩展 </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary> 是否已注册 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static bool IsRegisted<T>(this IServiceCollection services)
        {
            return services.IsRegisted(typeof(T));
        }

        /// <summary> 是否已注册 </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static bool IsRegisted(this IServiceCollection services, Type serviceType)
        {
            return services.Any(t => t.ServiceType == serviceType);
        }

        //public static T GetService<T>(this IServiceCollection services)
        //{
        //    var descriptor = services.FirstOrDefault(t => t.ServiceType == typeof(T));
        //    if (descriptor == null) return default;
        //    var instance = descriptor.ImplementationInstance;
        //    if (instance == null)
        //    {
        //        instance = descriptor.ImplementationFactory?.Invoke();
        //    }

        //    return instance == null ? default : (T)instance;
        //}

        ///// <summary> 添加监控 </summary>
        ///// <param name="services"></param>
        ///// <param name="configAction"></param>
        ///// <param name="monitorTypes"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddMonitor(this IServiceCollection services, Action<MonitorConfig> configAction = null, params Type[] monitorTypes)
        //{
        //    if (monitorTypes != null && monitorTypes.Any())
        //    {
        //        foreach (var monitorType in monitorTypes)
        //        {
        //            services.AddScoped(typeof(IMonitor), monitorType);
        //        }
        //    }
        //    services.AddSingleton<MonitorManager>();
        //    return services;
        //}

        ///// <summary> 监控记录 </summary>
        ///// <param name="provider"></param>
        ///// <param name="data"></param>
        //public static void Monitor(this IServiceProvider provider, MonitorData data)
        //{
        //    var manager = provider.GetService<MonitorManager>();
        //    manager?.Record(data);
        //}
    }
}
