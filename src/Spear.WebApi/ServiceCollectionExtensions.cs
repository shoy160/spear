using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Spear.Core.Extensions;
using Spear.Core.Reflection;
using Spear.Core.Serialize;
using Spear.WebApi.Activators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spear.WebApi
{
    public static class ServiceCollectionExtensions
    {
        /// <summary> 添加属性注入 </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddPropertyIoc(this IMvcBuilder builder)
        {
            var feature = new ControllerFeature();
            builder.PartManager.PopulateFeature(feature);
            foreach (var type in feature.Controllers.Select(c => c.AsType()))
                builder.Services.TryAddTransient(type, type);
            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, PropertyControllerActivator>());
            return builder;
        }

        private static SortedDictionary<string, Dictionary<int, string>> EnumsDict(this IServiceProvider provider, Func<Type, bool> filter = null)
        {
            if (filter == null)
                filter = type => type.Namespace.IsMatch("Spear\\..*\\.Contracts\\.Enums");

            var typeFinder = provider.GetService<ITypeFinder>();
            var enums = typeFinder.Find(type =>
                type.Namespace != null && type.IsEnum && filter(type));

            var dict = new SortedDictionary<string, Dictionary<int, string>>();
            foreach (var type in enums)
            {
                var values = new Dictionary<int, string>();
                var fields = type.GetFields().Where(t => t.IsLiteral).ToList();
                foreach (var item in Enum.GetValues(type))
                {
                    var field = fields.FirstOrDefault(t => t.Name == item.ToString());
                    var desc = field?.GetCustomAttribute<DescriptionAttribute>();
                    var key = item.CastTo<int>();
                    values.Add(key, desc?.Description ?? item.ToString());
                }

                dict.Add(!string.IsNullOrWhiteSpace(type.FullName) ? type.FullName : type.Name, values);
            }

            return dict;
        }

        /// <summary> 枚举路由 </summary>
        /// <param name="builder"></param>
        /// <param name="route">路由地址</param>
        /// <param name="filter"></param>
        /// <param name="duration">seconds</param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapEnums(this IEndpointRouteBuilder builder, string route = "/spear/enums",
            Func<Type, bool> filter = null, int duration = 30 * 60)
        {
            builder.MapGet(route, async ctx =>
            {
                var dict = ctx.RequestServices.EnumsDict(filter);
                if (duration > 0)
                {
                    var headers = ctx.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        Public = false,
                        MaxAge = TimeSpan.FromSeconds(duration)
                    };
                    //ctx.Response.Headers.TryAdd(HeaderNames.CacheControl, $"private,max-age={duration}");
                }

                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(JsonHelper.ToJson(dict), Encoding.UTF8);
            });
            return builder;
        }

        /// <summary> 替换 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providers"></param>
        /// <param name="target"></param>
        /// <param name="exist">必须存在</param>
        public static void Replace<T>(this IList<IModelBinderProvider> providers, IModelBinderProvider target, bool exist = true)
            where T : IModelBinderProvider
        {
            var source = providers.FirstOrDefault(t => t is T);
            if (exist && source == null) return;
            if (source != null)
            {
                var index = providers.IndexOf(source);
                providers.RemoveAt(index);
                providers.Insert(index, target);
            }
            else
            {
                providers.Add(target);
            }
        }
    }
}
