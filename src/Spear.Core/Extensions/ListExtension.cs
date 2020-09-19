using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Spear.Core.Helper;

namespace Spear.Core.Extensions
{
    /// <summary> 列表扩展 </summary>
    public static class ListExtension
    {
        /// <summary> 判断列表为空 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true;
            var array = source.ToArray();
            return !array.Any() || array.All(t => t == null || string.IsNullOrWhiteSpace(t.ToString()));
        }

        /// <summary>
        /// 遍历当前对象，并且调用方法进行处理
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="instance">实例</param>
        /// <param name="action">方法</param>
        /// <returns>当前集合</returns>
        public static void Each<T>(this IEnumerable<T> instance, Action<T> action)
        {
            if (action == null) return;
            foreach (var item in instance)
            {
                action(item);
            }
        }

        /// <summary> 遍历执行(自适应对象和列表) </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        public static void Each<T>(this T instance, Action<object> action)
        where T : class
        {
            if (action == null) return;
            if (instance != null && instance is IEnumerable list)
            {
                foreach (var item in list)
                {
                    action.Invoke(item);
                }
            }
            else
            {
                action.Invoke(instance);
            }
        }

        /// <summary>
        /// 比较数组相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源列表</param>
        /// <param name="target">目标列表</param>
        /// <param name="allowRepeat">是否允许重复</param>
        /// <returns></returns>
        public static bool ArrayEquals<T>(this IEnumerable<T> source, IEnumerable<T> target, bool allowRepeat = false)
        {
            if (source == null || target == null)
            {
                return source == null && target == null;
            }
            if (allowRepeat)
            {
                source = source.Distinct();
                target = target.Distinct();
            }
            var sourceList = source as IList<T> ?? source.ToList();
            var targetList = target as IList<T> ?? target.ToList();
            return sourceList.Count() == targetList.Count() && sourceList.All(targetList.Contains);
        }

        #region Distinct扩展

        /// <summary> 列表去重 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, TV>(this IEnumerable<T> source, Func<T, TV> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<T, TV>(keySelector));
        }

        /// <summary> 列表去重 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, TV>(this IEnumerable<T> source, Func<T, TV> keySelector, IEqualityComparer<TV> comparer)
        {
            return source.Distinct(new CommonEqualityComparer<T, TV>(keySelector, comparer));
        }

        /// <summary> DistinctBy 过滤扩展 </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }
        #endregion

        /// <summary>
        /// 遍历N次
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void Foreach(this int instance, Action<int> action)
        {
            for (var i = 0; i < instance; i++)
            {
                action(i);
            }
        }

        /// <summary>
        /// 列表遍历
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Foreach<T>(this IEnumerable<T> instance, Action<T> action)
        {
            foreach (var item in instance)
            {
                action(item);
            }
        }

        /// <summary>
        /// 以“,”拼接字符串
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable items)
        {
            return items.Join(",", "{0}");
        }

        /// <summary>
        /// 使用分隔符拼接字符串
        /// </summary>
        /// <param name="items"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable items, string separator)
        {
            return items.Join(separator, "{0}");
        }

        /// <summary>
        /// 使用分隔符、以及模板字符串拼接字符串
        /// </summary>
        /// <param name="items">待拼接集合</param>
        /// <param name="separator">分隔符</param>
        /// <param name="template">字符串格式化模板</param>
        /// <returns></returns>
        public static string Join(this IEnumerable items, string separator, string template)
        {
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (item == null) continue;
                sb.Append(separator);
                var type = item.GetType();
                if (type.IsValueType || type.Name == "String")
                    sb.Append(string.Format(template, item));
                else
                {
                    var temp = template;
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    temp = props.Aggregate(temp,
                        (current, prop) =>
                            new Regex("\\{" + prop.Name + "\\}").Replace(current, prop.GetValue(item, null).ToString()));
                    sb.Append(temp);
                }
            }
            return sb.ToString().Substring(separator.Length);
        }

        /// <summary> 随机排序 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomSort<T>(this IEnumerable<T> array)
        {
            return array.OrderBy(t => RandomHelper.Random().Next());
        }

        /// <summary> 是否包含在列表中 </summary>
        /// <param name="o"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool In(this object o, IEnumerable c)
        {
            return c.Cast<object>().Contains(o);
        }

        /// <summary>
        /// 是否包含在列表中
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool In<T>(this T t, params T[] c)
        {
            return c.Any(i => i.Equals(t));
        }

        /// <summary> 根据依赖项排序 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="getDependencies"></param>
        /// <returns></returns>
        public static IList<T> SortByDependencies<T>(this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getDependencies)
        {
            var sorted = new List<T>();
            var visitDict = new Dictionary<T, bool>();
            foreach (var item in source)
            {
                SortByDependenciesVisited(item, getDependencies, sorted, visitDict);
            }
            return sorted;
        }

        private static void SortByDependenciesVisited<T>(T item, Func<T, IEnumerable<T>> getDependencies,
            ICollection<T> sorted, IDictionary<T, bool> visitDict)
        {
            if (visitDict.TryGetValue(item, out var visited))
            {
                if (visited)
                {

                }
            }
            else
            {
                visitDict[item] = true;
                var dependencies = getDependencies(item);
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        SortByDependenciesVisited(dependency, getDependencies, sorted, visitDict);
                    }
                }
                visitDict[item] = false;
                sorted.Add(item);
            }
        }

        /// <summary> 去除数组中的null或者空字符 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="trimEmpty">是否清除空字符</param>
        /// <returns></returns>
        public static IEnumerable<T> Trim<T>(this IEnumerable<T> items, bool trimEmpty = true)
        {
            return items?.Where(item => item != null)
                .Where(item => !trimEmpty || !string.IsNullOrWhiteSpace(item.ToString()));
        }
    }
}
