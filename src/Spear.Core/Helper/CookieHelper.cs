using Spear.Core.Extensions;
using Spear.Core.Timing;

namespace Spear.Core.Helper
{
    /// <summary> cookie操作类 </summary>
    public static class CookieHelper
    {
        ///// <summary> 
        ///// 创建或修改COOKIE对象并赋Value值 
        ///// </summary> 
        ///// <param name="cookieName">COOKIE对象名</param> 
        ///// <param name="cookieExpired"> 
        ///// COOKIE对象有效时间（秒数）
        ///// 0表示会话cookie，负数表示删除 
        ///// </param> 
        ///// <param name="cookieDomain">作用域</param> 
        ///// <param name="cookieValue">COOKIE对象Value值</param> 
        ///// <remarks> 
        ///// 对COOKIE修改必须重新设Expires 
        ///// </remarks> 
        //public static void Set(string cookieName, string cookieValue, int cookieExpired = 0, string cookieDomain = null, string path = null, bool httpOnly = true)
        //{
        //    if (SpearHttpContext.Current == null) return;
        //    var key = cookieName;
        //    var value = cookieValue.UrlEncode();
        //    var options = new Microsoft.AspNetCore.Http.CookieOptions
        //    {
        //        HttpOnly = httpOnly
        //    };
        //    if (cookieExpired > 0)
        //        options.Expires = Clock.Now.AddSeconds(cookieExpired);
        //    else if (cookieExpired < 0)
        //    {
        //        options.Expires = Clock.Now.AddYears(-1);
        //    }
        //    if (!string.IsNullOrWhiteSpace(cookieDomain))
        //    {
        //        options.Domain = cookieDomain;
        //    }
        //    if (!string.IsNullOrWhiteSpace(path))
        //    {
        //        options.Path = path;
        //    }
        //    SpearHttpContext.Current.Response.Cookies.Append(key, value, options);
        //}

        ///// <summary> 
        ///// 读取Cookie某个对象的某个Key键的键值 
        ///// </summary> 
        ///// <param name="cookieName">Cookie对象名称</param> 
        ///// <param name="strKeyName">Key键名</param> 
        ///// <returns>Key键值，如果对象或键值就不存在，则返回 null</returns> 
        //public static string GetValue(string cookieName)
        //{
        //    if (!SpearHttpContext.Current.Request.Cookies.TryGetValue(cookieName, out var cookie))
        //        return string.Empty;
        //    return cookie.UrlDecode();
        //}

        ///// <summary> 
        ///// 删除某个COOKIE对象某个Key子键 
        ///// </summary> 
        ///// <param name="cookieName">Cookie对象名称</param> 
        ///// <param name="strKeyName">Key键名</param> 
        ///// <param name="iExpires"> 
        ///// COOKIE对象有效时间（秒数） 
        ///// 1表示永久有效，0和负数都表示不设有效时间 
        ///// 大于等于2表示具体有效秒数 
        ///// 86400秒 = 1天 = （60*60*24） 
        ///// 604800秒 = 1周 = （60*60*24*7） 
        ///// 2593000秒 = 1月 = （60*60*24*30） 
        ///// 31536000秒 = 1年 = （60*60*24*365） 
        ///// </param>
        ///// <param name="cookieDomain"></param>
        ///// <returns>如果对象本就不存在，则返回 false</returns> 
        //public static bool Delete(string cookieName, string cookieDomain = null)
        //{
        //    if (!SpearHttpContext.Current.Request.Cookies.TryGetValue(cookieName, out var cookie))
        //        return true;
        //    Set(cookieName, string.Empty, -1, cookieDomain);
        //    return true;
        //}

        ///// <summary>
        ///// 获取多少小时
        ///// </summary>
        ///// <param name="hours"></param>
        ///// <returns></returns>
        //public static int Hours(this int hours)
        //{
        //    return 60 * 60 * hours;
        //}

        ///// <summary>
        ///// 获取多少天
        ///// </summary>
        ///// <param name="day"></param>
        ///// <returns></returns>
        //public static int Days(this int day)
        //{
        //    return 60 * 60 * 24 * day;
        //}

        ///// <summary>
        ///// 获取多少月
        ///// </summary>
        ///// <param name="months"></param>
        ///// <returns></returns>
        //public static int Months(this int months)
        //{
        //    return 60 * 60 * 24 * 30 * months;
        //}
    }
}
