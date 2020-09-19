using Spear.Core.Extensions;
using System;

namespace Spear.Dapper.Config
{
    public static class ConfigExtensions
    {
        /// <summary> 获取配置常量 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T Const<T>(this string key, T def = default)
        {
            return string.IsNullOrWhiteSpace(key) ? def : $"const:{key}".Config(def);
        }

        /// <summary> 获取站点 </summary>
        /// <param name="site"></param>
        /// <param name="relativeUrl"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string Site(this string site, string relativeUrl = null, string def = default)
        {
            if (string.IsNullOrWhiteSpace(site))
                return relativeUrl;
            var baseUri = $"sites:{site}".Config(def);
            return relativeUrl != null ? new Uri(new Uri(baseUri), relativeUrl).AbsoluteUri : baseUri;
        }

        /// <summary> 获取站点 </summary>
        /// <param name="site"></param>
        /// <param name="relativeUrl"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string Site(this Enum site, string relativeUrl = null, string def = default)
        {
            return site.ToString().ToCamelCase().Site(relativeUrl, def);
        }
    }
}
