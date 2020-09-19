using Spear.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spear.Core.Helper
{
    /// <summary> 正则辅助类 </summary>
    public static class RegexHelper
    {
        private const string BrRegex = @"(\r|\n)";
        private const string TrnRegex = @"(\r|\n|\t)";
        private const string DomainRegex = @".(\w+).(com.cn|net.cn|org.cn|edu.cn|com|net|org|cn|biz|info|cc|tv)";
        private const string IpRegex = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

        private const string UrlRegex =
            @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$";
        /// <summary> 手机号码 </summary>
        private const string MobileRegex = @"^1[0-9]{10}$";//@"^(0[0-9]{2,3}-?[0-9]{7,8})|((13|15|18)\d{9})$";
        /// <summary> 邮箱 </summary>
        private const string EmailRegex = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        /// <summary> 身份证 </summary>
        private const string IdCardRegex =
            @"^[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$)|(^[1-9]\d{5}\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{2}$";

        private const string HtmlTagRegex = @"</?[0-9a-zA-Z]+[^>]*/?>";
        private const string FloatRegex = @"^([-]|[0-9])[0-9]*(\.\w*)?$";
        /// <summary> 车牌号码 </summary>
        private const string PlateNumberRegex =
            @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[A-Z0-9]{4,5}[A-Z0-9挂学警港澳]{1}$";

        private const string HtmlFindByIdRegex =
            @"<([0-9a-zA-Z]+)[^>]*\bid=([""']){0}\2[^>]*>(?><\1[^>]*>(?<tag>)|</\1>(?<-tag>)|.)*?(?(tag)(?!))</\1>";

        private const string HtmlIdRegex = @"<([0-9a-zA-Z]+)[^>]*\bid=([""']){0}\2[^>]*/>";

        private const string HtmlFindByCssRegex =
            @"<([0-9a-zA-Z]+)[^>]*\bclass=(['""]?)(?<t>[^""'\s]*\s)*{0}(?<b>\s[^""'\s]*)*\2[^>]*>(?><\1[^>]*>(?<tag>)|</\1>(?<-tag>)|.)*?(?(tag)(?!))</\1>";
        private const string HtmlCssRegex = @"<([0-9a-zA-Z]+)[^>]*\bclass=(['""]?)(?<t>[^""'\s]*\s)*{0}(?<b>\s[^""'\s]*)*\2[^>]*/>";
        private const string HtmlFindByAttrRegex = @"<([0-9a-zA-Z]+)[^>]*\b{0}[^>]*>(?><\1[^>]*>(?<tag>)|</\1>(?<-tag>)|.)*?(?(tag)(?!))</\1>";
        private const string HtmlAttrRegex = @"<([0-9a-zA-Z]+)[^>]*\b{0}[^>]*/>";

        /// <summary>
        /// 获取单个正则匹配的字符
        /// </summary>
        /// <param name="regex">正则</param>
        /// <param name="str">字符串</param>
        /// <param name="ops">表达式选项</param>
        /// <param name="group">组</param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string Match(string str, string regex, RegexOptions ops, int group = 1, string groupName = null)
        {
            var reg = new Regex(regex, ops);
            var m = reg.Match(str);
            return !string.IsNullOrWhiteSpace(groupName) ? m.Groups[groupName].Value : m.Groups[@group].Value;
        }

        /// <summary>
        /// (简化)获取正则匹配的字符
        /// </summary>
        /// <param name="regex">正则</param>
        /// <param name="str">字符串</param>
        /// <param name="group">组</param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static string Match(string str, string regex, int group = 1, string groupName = null)
        {
            return Match(str, regex, RegexOptions.Compiled, group, groupName);
        }

        public static List<string> Matches(string docHtml, string regStr, RegexOptions options, int index = 1,
            string groupName = null)
        {
            var mts = (new Regex(regStr, options)).Matches(docHtml);
            return !string.IsNullOrWhiteSpace(groupName)
                ? (from Match mt in mts select mt.Groups[groupName].Value).ToList()
                : (from Match mt in mts select mt.Groups[index].Value).ToList();
        }

        public static List<string> Matches(string docHtml, string regStr, int index = 1, string groupName = null)
        {
            return Matches(docHtml, regStr, RegexOptions.IgnoreCase | RegexOptions.Singleline, index, groupName);
        }

        /// <summary>
        /// 清除给定字符串中的回车及换行符
        /// </summary>
        /// <param name="str">要清除的字符串</param>
        /// <returns>清除后返回的字符串</returns>
        public static string ClearBr(string str)
        {
            return string.IsNullOrEmpty(str) ? str : Regex.Replace(str, BrRegex, string.Empty);
        }

        /// <summary>
        /// 清除\r \n \t
        /// </summary>
        /// <param name="str">str</param>
        /// <returns></returns>
        public static string ClearTrn(string str)
        {
            return string.IsNullOrEmpty(str) ? str : Regex.Replace(str, TrnRegex, string.Empty);
        }

        /// <summary>
        /// 获取域名
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static string GetDomain(string hostName)
        {
            var reg = new Regex(DomainRegex);
            if (hostName == null)
                hostName = string.Empty;
            return reg.Match(hostName).Value;
        }

        /// <summary>
        /// 是否是手机号码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(string mobile)
        {
            return !string.IsNullOrWhiteSpace(mobile) && Regex.IsMatch(mobile, MobileRegex);
        }

        /// <summary>
        /// 是否是邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, EmailRegex);
        }

        /// <summary>
        /// 是否是IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIp(string ip)
        {
            return !string.IsNullOrWhiteSpace(ip) && Regex.IsMatch(ip, IpRegex);
        }

        ///<summary>
        /// 判断是否是url
        ///</summary>
        ///<param name="strUrl"></param>
        ///<returns></returns>
        public static bool IsUrl(string strUrl)
        {
            return !string.IsNullOrWhiteSpace(strUrl) && Regex.IsMatch(strUrl, UrlRegex);
        }

        /// <summary>
        /// 是否是浮点字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsFloat(string str)
        {
            return !string.IsNullOrWhiteSpace(str) && Regex.IsMatch(str, FloatRegex);
        }

        /// <summary> 是否是身份证号码 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsIdCardNo(string str)
        {
            return !string.IsNullOrWhiteSpace(str) && Regex.IsMatch(str, IdCardRegex);
        }

        /// <summary> 是否是车牌号 </summary>
        /// <param name="plateNumber"></param>
        /// <returns></returns>
        public static bool IsPlateNumber(string plateNumber)
        {
            return !string.IsNullOrWhiteSpace(plateNumber) &&
                   Regex.IsMatch(plateNumber, PlateNumberRegex);
        }

        /// <summary> 车架号校验 </summary>
        /// <param name="vinNumber"></param>
        /// <returns></returns>
        public static bool IsVinNumber(string vinNumber)
        {
            if (vinNumber.IsNullOrEmpty() || vinNumber.Length != 17)
                return false;
            vinNumber = vinNumber.ToUpper();

            //车架号不会包含“I,O,Q”
            if (Regex.IsMatch(vinNumber, @"[IOQ]"))
                return false;
            if (!Regex.IsMatch(vinNumber, @"[A-Z0-9]{8}\d[A-Z0-9]{3}\d{5}"))
                return false;

            //加权值
            var weightList = new[] { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };

            //对应值
            var contrastDict = new Dictionary<char, int>
            {
                {'0', 0},
                {'1', 1},
                {'2', 2},
                {'3', 3},
                {'4', 4},
                {'5', 5},
                {'6', 6},
                {'7', 7},
                {'8', 8},
                {'9', 9},
                {'A', 1},
                {'B', 2},
                {'C', 3},
                {'D', 4},
                {'E', 5},
                {'F', 6},
                {'G', 7},
                {'H', 8},
                {'J', 1},
                {'K', 2},
                {'L', 3},
                {'M', 4},
                {'N', 5},
                {'P', 7},
                {'R', 9},
                {'S', 2},
                {'T', 3},
                {'U', 4},
                {'V', 5},
                {'W', 6},
                {'X', 7},
                {'Y', 8},
                {'Z', 9}
            };

            var sum = 0;
            var chars = vinNumber.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (i == 8) continue;
                var c = chars[i];
                if (!contrastDict.ContainsKey(c))
                    return false;
                sum += contrastDict[c] * weightList[i];
            }

            return sum % 11 == int.Parse(chars[8].ToString());

        }

        /// <summary> 清除Html标签 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ClearHtml(string str)
        {
            return Regex.Replace(str, HtmlTagRegex, string.Empty);
        }

        public static string FindById(string html, string id)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
            html = ClearTrn(html);
            var reg = HtmlFindByIdRegex.FormatWith(id);
            if (Regex.IsMatch(html, reg))
                reg = HtmlIdRegex.FormatWith(id);
            return Match(html, reg, RegexOptions.Singleline | RegexOptions.IgnoreCase, 0);
        }

        public static IEnumerable<string> FindByCss(string html, string css)
        {
            if (string.IsNullOrEmpty(html))
                return new List<string>();
            html = ClearTrn(html);
            var reg = HtmlFindByCssRegex.FormatWith(css);
            if (Regex.IsMatch(html, reg))
                reg = HtmlCssRegex.FormatWith(css);
            return Matches(html, reg, 0);
        }

        public static IEnumerable<string> FindByAttr(string html, string attr)
        {
            if (string.IsNullOrEmpty(html))
                return new List<string>();
            html = ClearTrn(html);
            var reg = HtmlFindByAttrRegex.FormatWith(attr);
            if (Regex.IsMatch(html, reg))
                reg = HtmlAttrRegex.FormatWith(attr);
            return Matches(html, reg, 0);
        }
    }
}
