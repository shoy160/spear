using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Extensions;
using System;
using System.Reflection;

namespace Spear.Core.Dependency
{
    public static class PropertyIocExtensions
    {
        /// <summary> 属性注入填充 </summary>
        /// <param name="target"></param>
        /// <param name="provider"></param>
        private static void PropSet(object target, IServiceProvider provider)
        {
            var props = target.GetType().GetTypeInfo().DeclaredProperties;
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<AutowiredAttribute>() != null)
                {
                    var propValue = provider.GetService(prop.PropertyType);
                    if (propValue != null)
                        prop.SetValue(target, propValue);
                }

                if (prop.GetCustomAttribute<ConfigAttribute>() is ConfigAttribute configAttribute)
                {
                    var propValue = configAttribute.Key.Config(prop.PropertyType);
                    if (propValue != null)
                        prop.SetValue(target, propValue);
                }
            }
        }

        /// <summary> 字段注入填充 </summary>
        /// <param name="target"></param>
        /// <param name="provider"></param>
        private static void FieldSet(object target, IServiceProvider provider)
        {
            var props = target.GetType().GetTypeInfo().DeclaredFields;
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<AutowiredAttribute>() != null)
                {
                    var propValue = provider.GetService(prop.FieldType);
                    if (propValue != null)
                        prop.SetValue(target, propValue);
                }

                if (prop.GetCustomAttribute<ConfigAttribute>() is ConfigAttribute configAttribute)
                {
                    var propValue = configAttribute.Key.Config(prop.FieldType);
                    if (propValue != null)
                        prop.SetValue(target, propValue);
                }
            }
        }

        public static object PropResolve(this IServiceProvider provider, Type serviceType)
        {
            var target = provider.GetRequiredService(serviceType);
            PropSet(target, provider);
            FieldSet(target, provider);
            return target;
        }

        /// <summary> 获取Ioc实例(包含属性注入) </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T PropResolve<T>(this IServiceProvider provider)
        {
            var target = provider.GetRequiredService<T>();
            PropSet(target, provider);
            FieldSet(target, provider);
            return target;
        }

        /// <summary> 添加属性服务 </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceCollection AddPropService(this IServiceCollection services, Type serviceType,
            Type implementationType, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));
            var serviceDescriptor = new ServiceDescriptor(serviceType, provider =>
            {
                var target = provider.GetService(implementationType);
                PropSet(target, provider);
                FieldSet(target, provider);
                return target;
            }, lifetime);
            services.Add(serviceDescriptor);
            return services;
        }
    }
}
