using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using Spear.Core.Context;

namespace Spear.Core.Extensions
{
    public static class HttpContextExtension
    {
        /// <summary> Wrap </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static HttpContextWrap Wrap(this HttpContext context)
        {
            return new HttpContextWrap(context);
        }

        /// <summary> 获取该字符串的QueryString值 </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="context"></param>
        /// <param name="str">字符串</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T Query<T>(this HttpContext context, string str, T def)
        {
            try
            {
                var qs = context.Request.Query[str].ToString();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary> 获取该字符串的Form值 </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="context"></param>
        /// <param name="str">字符串</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T Form<T>(this HttpContext context, string str, T def)
        {
            try
            {
                var qs = context.Request.Form[str].ToString();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary> 获取该字符串QueryString或Form值 </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="context"></param>
        /// <param name="str"></param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static T QueryOrForm<T>(this HttpContext context, string str, T def)
        {
            try
            {
                var qs = context.Request.Query.ContainsKey(str) ? context.Request.Query[str].ToString() : context.Request.Form[str].ToString();
                return qs.CastTo(def);
            }
            catch
            {
                return def;
            }
        }

        /// <summary> 获取原始Url </summary>
        /// <returns></returns>
        public static string RawUrl(this HttpContext context)
        {
            if (context?.Request == null)
                return string.Empty;
            try
            {
                var request = context.Request;
                return $"{request.Scheme}://{request.Host}{request.Path.Value}{request.QueryString.Value}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary> 设置参数 </summary>
        /// <param name="context"></param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static string SetQuery(this HttpContext context, string key, object value, string url = null)
        {
            if (url.IsNullOrEmpty())
            {
                url = context.RawUrl();
            }
            if (string.IsNullOrWhiteSpace(url) || key.IsNullOrEmpty())
                return url;
            value = value ?? string.Empty;
            var qs = url.Split('?');
            var list = new NameValueCollection();
            if (qs.Length < 2)
            {
                list.Add(key, value.ToString().UrlEncode());
            }
            else
            {
                foreach (var query in qs[1].Split('&'))
                {
                    var item = query.Split('=');
                    if (item.Length == 2)
                        list.Add(item[0], item[1]);
                }
                list[key] = value.ToString().UrlEncode();
            }
            var search = string.Empty;
            for (var i = 0; i < list.AllKeys.Length; i++)
            {
                search += list.AllKeys[i] + "=" + list[i];
                if (i < list.Count - 1)
                    search += "&";
            }
            return qs[0] + "?" + search;
        }
    }
}
