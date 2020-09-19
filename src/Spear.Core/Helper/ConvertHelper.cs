using Spear.Core.Extensions;
using System;
using System.Globalization;
using System.Reflection;

namespace Spear.Core.Helper
{
    /// <summary> 类型转换辅助 </summary>
    public static class ConvertHelper
    {
        private static readonly DateTime DefaultTime = DateTime.Parse("1900-01-01");
        /// <summary>
        /// string转换为float
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns></returns>
        public static float StrToFloat(string strValue, float defValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
                return defValue;

            float intValue = defValue;
            bool isFloat = RegexHelper.IsFloat(strValue);
            if (isFloat)
                float.TryParse(strValue, out intValue);
            return intValue;
        }

        /// <summary>
        /// object转化为float
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static float StrToFloat(object obj, float defValue)
        {
            if (obj == null)
                return defValue;
            return StrToFloat(obj.ToString(), defValue);
        }

        /// <summary>
        /// string转化为int
        /// </summary>
        /// <param name="str">字符</param>
        /// <param name="defValue">默认值</param>
        /// <returns></returns>
        public static int StrToInt(string str, int defValue)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 ||
                !RegexHelper.IsFloat(str.Trim()))
                return defValue;

            int rv;
            if (Int32.TryParse(str, out rv))
                return rv;

            return Convert.ToInt32(StrToFloat(str, defValue));
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static int StrToInt(object obj, int defValue)
        {
            if (obj == null)
                return defValue;
            return StrToInt(obj.ToString(), defValue);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime StrToDateTime(string str, DateTime defValue)
        {
            if (!string.IsNullOrEmpty(str))
            {
                DateTime dateTime;
                if (DateTime.TryParse(str, out dateTime))
                    return dateTime;
            }
            return defValue;
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static DateTime StrToDateTime(object obj, DateTime defValue)
        {
            if (obj == null) return defValue;
            return StrToDateTime(obj.ToString(), defValue);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime StrToDateTime(string str)
        {
            return StrToDateTime(str, DefaultTime);
        }

        /// <summary> 相同属性不同类转换 </summary>
        /// <typeparam name="T">转换目标类</typeparam>
        /// <returns></returns>
        public static T ClassConvert<T>(object source)
            where T : new()
        {
            var target = new T();
            var sourceType = source.GetType();
            var targetType = target.GetType();
            var targetProps =
                targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            foreach (PropertyInfo targetProp in targetProps)
            {
                var sourceProp = sourceType.GetProperty(targetProp.Name);
                if (sourceProp == null || sourceProp.PropertyType != targetProp.PropertyType)
                    continue;
                var pv = sourceProp.GetValue(source, null);
                targetProp.SetValue(target, pv, null);
            }
            return target;
        }

        /// <summary>
        /// 获取数字中文
        /// 不完善
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToBigNumber(long num)
        {
            var word = new[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            var dw = new[] { "十", "百", "千", "万", "亿" };
            var desc = string.Empty;
            var str = num.ToString(CultureInfo.InvariantCulture).Reverse();
            //1000,000)
            for (var i = 0; i < str.Length; i++)
            {
                var n = (StrToInt(str[i], 0));
                if (n > 0 || (i > 0 && (i - 4) % 4 == 0))
                {
                    if ((i - 1) % 4 == 0)
                        desc = dw[0] + desc;
                    else if ((i - 2) % 4 == 0)
                        desc = dw[1] + desc;
                    else if ((i - 3) % 4 == 0)
                        desc = dw[2] + desc;
                    else if (i > 3 && (i - 4) % 8 == 0)
                        desc = dw[3] + desc;
                    else if (i > 7 && i % 8 == 0)
                        desc = dw[4] + desc;
                }
                if ((i == 0 && n == 0) || (n == 0 && (desc.StartsWith(dw[3]) || desc.StartsWith(dw[4]))))
                    continue;
                if (!desc.StartsWith(word[0]) && (n != 0 || ((i - 4) % 4 != 0)))
                    desc = word[n] + desc;
            }
            return desc.TrimEnd(word[0].ToCharArray());
        }

        /// <summary>
        /// 私有36进制字符配置
        /// </summary>
        private static readonly string[] DecimalSystem36Array =
            new string[36] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        /// <summary>
        /// 10进制转换为36进制
        /// </summary>
        /// <param name="num">正整数</param>
        /// <returns>36进制字符串</returns>
        public static string Ds10ToDs36(int num)
        {
            // 考虑过用uint,调用最多的还是int,避免转换
            if (num < 0) num = 0;
            int z = 36, x = num / z, y = num % z;
            var s = DecimalSystem36Array[y];
            if (x != 0) s = Ds10ToDs36(x) + s;
            return s;
        }

        /// <summary>
        /// 36进制转换为10进制
        /// </summary>
        /// <param name="str">36进制字符串</param>
        /// <returns>正整数</returns>
        public static int Ds36ToDs10(string str)
        {
            // B => 11
            // BBB => 36*36*11 + 36*11 +36*0 + 11
            int z = 36, len = str.Length;
            str = str.ToUpper();
            //先计算个位数
            var result = Array.IndexOf(DecimalSystem36Array, str[len - 1].ToString());
            if (result == -1) return 0;
            //无需计算个位数“36*0”（36的0次方）
            for (int i = 0; i < len - 1; i++)
            {
                var tmpCharIdx = Array.IndexOf(DecimalSystem36Array, str[i].ToString());
                if (tmpCharIdx == -1) return 0;
                result += (int)(Math.Pow(z, len - 1 - i)) * tmpCharIdx;
            }
            return result;
        }
    }
}
