using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Spear.Core.Extensions
{
    /// <summary> 枚举相关扩展 </summary>
    public static class EnumExtension
    {
        private const string DefaultEnumText = "--";

        /// <summary> 获取枚举值 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int GetValue<T>(this T t)
            where T : struct
        {
            var type = typeof(T);
            if (!type.IsEnum)
                return default(int);
            try
            {
                return (int)(object)t;
            }
            catch
            {
                return default(int);
            }
        }

        /// <summary> 获取枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tp"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetText<T>(this T tp, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
        {
            try
            {
                var ms = tp.GetType();
                if (!ms.IsEnum || !(tp is Enum))
                    return defText;
                var names = new List<string>();
                if (!isFlag || tp.CastTo(0) <= 0)
                {
                    var field = ms.GetField(tp.ToString());
                    return field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name;
                }

                foreach (var value in Enum.GetValues(ms))
                {
                    //排除非位运算枚举
                    if (value.CastTo(0) <= 0)
                        continue;
                    if (value is Enum @enum && (tp as Enum).HasFlag(@enum))
                        names.Add(@enum.ToString());
                }

                var fields = ms.GetFields().Where(t => names.Contains(t.Name)).ToArray();
                if (!fields.Any())
                    return defText;
                var list = fields.Select(f => f.GetCustomAttribute<DescriptionAttribute>()?.Description ?? f.Name);
                return string.Join(",", list);
            }
            catch
            {
                return defText;
            }
        }

        private static readonly string[] FontColors = new[] { "Gray", "Black", "Green", "Blue", "Fuchsia", "Red" };
        private const string EnumHtml = "<font color='{0}'>{1}</font>";

        /// <summary> 获取带颜色值的枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tp"></param>
        /// <param name="colors">颜色值</param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumCssText<T>(this T tp, string[] colors, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
        {
            var types = tp.GetType().GetFields().Where(t => t.IsLiteral).ToList();
            var index = types.IndexOf(types.FirstOrDefault(t => t.Name == tp.ToString()));
            if (index >= 0)
                return string.Format(EnumHtml, colors[(index >= colors.Length ? (colors.Length - 1) : index)],
                    tp.GetText(isFlag, defText));
            return string.Empty;
        }

        /// <summary> 获取带颜色值的枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tp"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumCssText<T>(this T tp, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
        {
            return tp.GetEnumCssText(FontColors);
        }

        /// <summary> 获取枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="type"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumText<T, TV>(this TV type, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T)(object)type).GetText(isFlag, defText);
            }
            catch
            {
                return DefaultEnumText;
            }
        }

        /// <summary> 获取枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumText<T>(this int type, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
        {
            return type.GetEnumText<T, int>(isFlag, defText);
        }

        /// <summary> 获取带颜色的枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="type"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumCssText<T, TV>(this TV type, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
            where TV : struct
        {
            try
            {
                return ((T)(object)type).GetEnumCssText(isFlag, defText);
            }
            catch
            {
                return DefaultEnumText;
            }
        }

        /// <summary> 获取带颜色的枚举描述 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="isFlag">是否Flag,Flag将返回多个描述,并以","分割</param>
        /// <param name="defText">默认描述</param>
        /// <returns></returns>
        public static string GetEnumCssText<T>(this int type, bool isFlag = false, string defText = DefaultEnumText)
            where T : struct
        {
            return type.GetEnumCssText<T, int>(isFlag, defText);
        }

        /// <summary> 转换为枚举类型 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T ToEnum<T, TV>(this TV type)
            where T : struct
            where TV : struct
        {
            try
            {
                return (T)(object)type;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary> 转换为枚举类型 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this int type)
            where T : struct
        {
            return type.ToEnum<T, int>();
        }
    }
}
