using Spear.Core.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spear.Core.Extensions
{
    /// <summary> 正则相关扩展 </summary>
    public static class RegexExtension
    {
        public static Regex ToRegex(this string c, RegexOptions opts)
        {
            return new Regex(c, opts);
        }

        public static Regex ToRegex(this string c)
        {
            return new Regex(c, RegexOptions.Compiled);
        }

        public static bool IsMatch(this string c, string pattern, RegexOptions opts)
        {
            if (c.IsNullOrEmpty()) return false;
            return Regex.IsMatch(c, pattern, opts);
        }

        public static bool IsMatch(this string c, string pattern)
        {
            return c.IsMatch(pattern, RegexOptions.None);
        }

        public static string Match(this string c, string pattern)
        {
            return c.Match(pattern, 0);
        }

        public static string Match(this string c, string pattern, int index)
        {
            return c.Match(pattern, index, RegexOptions.None);
        }

        public static string Match(this string c, string pattern, int index, RegexOptions opts)
        {
            return c.IsNullOrEmpty() ? string.Empty : Regex.Match(c, pattern, opts).Groups[index].Value;
        }

        public static string Match(this string c, string pattern, string groupName, RegexOptions opts)
        {
            return c.IsNullOrEmpty() ? string.Empty : Regex.Match(c, pattern, opts).Groups[groupName].Value;
        }

        public static string Match(this string c, string pattern, string groupName)
        {
            return c.Match(pattern, groupName, RegexOptions.None);
        }

        public static IEnumerable<string> Matches(this string c, string parent)
        {
            return c.Matches(parent, 1, RegexOptions.None);
        }

        public static IEnumerable<string> Matches(this string c, string parent, int index, RegexOptions opts)
        {
            var list = new List<string>();
            if (c.IsNullOrEmpty())
                return list;
            var ms = Regex.Matches(c, parent, opts);
            list.AddRange(from Match m in ms select m.Groups[index].Value);
            return list;
        }

        public static IEnumerable<string> Matches(this string c, string parent, string groupName, RegexOptions opts)
        {
            var list = new List<string>();
            if (c.IsNullOrEmpty())
                return list;
            var ms = Regex.Matches(c, parent, opts);
            list.AddRange(from Match m in ms select m.Groups[groupName].Value);
            return list;
        }

        public static string Replace(this string c, string parent, string replaceMent, int count, int startAt, RegexOptions opts)
        {
            var reg = parent.ToRegex(opts);
            if (count <= 0)
                return reg.Replace(c, replaceMent);
            return startAt >= 0
                ? reg.Replace(c, replaceMent, count, startAt)
                : reg.Replace(c, replaceMent, count);
        }

        public static string Replace(this string c, string parent, string replaceMent, RegexOptions opts)
        {
            return c.Replace(parent, replaceMent, 0, -1, opts);
        }

        public static string Replace(this string c, string parent, string replaceMent)
        {
            return c.Replace(parent, replaceMent, RegexOptions.Compiled);
        }

        /// <summary> 是否是邮箱 </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsEmail(this string c)
        {
            return RegexHelper.IsEmail(c);
        }

        /// <summary> 是否是IP </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsIp(this string c)
        {
            return RegexHelper.IsIp(c);
        }

        /// <summary> 判断是否是url </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsUrl(this string c)
        {
            return RegexHelper.IsUrl(c);
        }

        /// <summary> 是否是手机号码 </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsMobile(this string c)
        {
            return RegexHelper.IsMobile(c);
        }

        /// <summary> 是否是浮点字符 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsFloat(this string str)
        {
            return RegexHelper.IsFloat(str);
        }

        /// <summary> 是否是身份证号码 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsIdCardNo(string str)
        {
            return RegexHelper.IsIdCardNo(str);
        }

        /// <summary> 是否是车牌号 </summary>
        /// <param name="plateNumber"></param>
        /// <returns></returns>
        public static bool IsPlateNumber(string plateNumber)
        {
            return RegexHelper.IsPlateNumber(plateNumber);
        }

        /// <summary> 车架号校验 </summary>
        /// <param name="vinNumber"></param>
        /// <returns></returns>
        public static bool IsVinNumber(string vinNumber)
        {
            return RegexHelper.IsVinNumber(vinNumber);
        }

        /// <summary>
        /// base64验证
        /// * 字符串只可能包含A-Z，a-z，0-9，+，/，=字符
        /// * 字符串长度是4的倍数
        /// * =只会出现在字符串最后，可能没有或者一个等号或者两个等号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsBase64(this string source)
        {
            const string base64Pattern = "^([0-9a-z+/]{4})*([0-9a-z+/]{4}|[0-9a-z+/]{3}=|[0-9a-z+/]{2}==)$";
            return source.IsMatch(base64Pattern, RegexOptions.IgnoreCase);
        }
    }
}
