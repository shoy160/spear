using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#endif

namespace Spear.Sdk.Core
{
    /// <summary> 扩展类 </summary>
    public static class Extensions
    {
        /// <summary> 起始时间 </summary>
        private static readonly DateTime ZoneTime = new DateTime(1970, 1, 1);

        /// <summary> 时间戳 </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static long Timestamp(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc)
        {
            switch (kind)
            {
                case DateTimeKind.Utc:
                    dateTime = dateTime.ToUniversalTime();
                    break;
                case DateTimeKind.Local:
                    dateTime = dateTime.ToLocalTime();
                    break;
            }
            var timespan = dateTime.ToUniversalTime() - ZoneTime;
            return (long)timespan.TotalMilliseconds;
        }

        /// <summary> 时间戳转日期 </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime Datetime(this long timestamp)
        {
            return ZoneTime.Add(new TimeSpan(timestamp * TimeSpan.TicksPerMillisecond)).ToLocalTime();
        }

        /// <summary> 长整型转换 </summary>
        /// <param name="str"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static long ToLong(this string str, long def = 0)
        {
            return long.TryParse(str, out var value) ? value : def;
        }

        /// <summary>
        /// 判断类型是否为Nullable类型
        /// </summary>
        /// <param name="type"> 要处理的类型 </param>
        /// <returns> 是返回True，不是返回False </returns>
        public static bool IsNullableType(this Type type)
        {
            return ((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary> MD5 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var algorithm = MD5.Create();
            algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(algorithm.Hash).Replace("-", string.Empty).ToUpper();
        }

        public static string Stringfy(this object param, bool filterEmpty = true, Encoding encoding = null)
        {
            if (param == null) return string.Empty;
            var props = param.GetType().GetProperties();
            var query = new List<string>();
            foreach (var prop in props)
            {
                var value = prop.GetValue(param);
                if (value == null)
                {
                    if (filterEmpty)
                        continue;
                    query.Add($"{prop.Name}=");
                }
                else
                {
                    var queryValue = encoding != null
                        ? HttpUtility.UrlEncode(value.ToString(), encoding)
                        : value.ToString();
                    query.Add($"{prop.Name}={queryValue}");
                }
            }

            return string.Join("&", query);
        }
        private const string DefaultIp = "127.0.0.1";

        public static bool IsIp(this string ipAddress)
        {
            return Regex.IsMatch(ipAddress, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

#if NETSTANDARD2_0
        public static string ClientIp(this HttpContext httpContext)
        {
            return DefaultIp;
        }
#else
        /// <summary> 获取IP地址 </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string ClientIp(this HttpContext context)
        {
            if (context == null)
                return DefaultIp;
            return DefaultIp;
        }
#endif
    }
}
