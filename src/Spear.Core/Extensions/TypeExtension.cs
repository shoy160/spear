using Spear.Core.Serialize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Extensions
{
    /// <summary>
    /// 类型<see cref="Type"/>辅助扩展方法类
    /// </summary>
    public static class TypeExtension
    {
        private static readonly List<Type> SimpleTypes = new List<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(byte[])
        };

        /// <summary>
        /// 判断类型是否为Nullable类型
        /// </summary>
        /// <param name="type"> 要处理的类型 </param>
        /// <returns> 是返回True，不是返回False </returns>
        public static bool IsNullableType(this Type type)
        {
            return ((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// 由类型的Nullable类型返回实际类型
        /// </summary>
        /// <param name="type"> 要处理的类型对象 </param>
        /// <returns> </returns>
        public static Type GetNonNummableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        /// <summary>
        /// 通过类型转换器获取Nullable类型的基础类型
        /// </summary>
        /// <param name="type"> 要处理的类型对象 </param>
        /// <returns> </returns>
        public static Type GetUnNullableType(this Type type)
        {
            if (!IsNullableType(type)) return type;
            var nullableConverter = new NullableConverter(type);
            return nullableConverter.UnderlyingType;
        }

        /// <summary> 是否是简单类型 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            var actualType = type.GetUnNullableType();
            return SimpleTypes.Contains(actualType);
        }

        /// <summary> 是否是Task OR Void </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTaskOrVoid(this Type type)
        {
            var currentType = type;
            if (typeof(Task<>).IsGenericAssignableFrom(type))
            {
                currentType = type.GenericTypeArguments[0];
            }

            return currentType == typeof(void) || currentType == typeof(Task);
        }

        /// <summary>
        /// 获取成员元数据的Description特性描述信息
        /// </summary>
        /// <param name="member">成员元数据对象</param>
        /// <param name="inherit">是否搜索成员的继承链以查找描述特性</param>
        /// <returns>返回Description特性描述信息，如不存在则返回成员的名称</returns>
        public static string ToDescription(this MemberInfo member, bool inherit = false)
        {
            var desc = member.GetAttribute<DescriptionAttribute>(inherit);
            return desc == null ? member.Name : desc.Description;
        }

        /// <summary>
        /// 检查指定指定类型成员中是否存在指定的Attribute特性
        /// </summary>
        /// <typeparam name="T">要检查的Attribute特性类型</typeparam>
        /// <param name="memberInfo">要检查的类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>是否存在</returns>
        public static bool AttributeExists<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Any(m => (m as T) != null);
        }

        /// <summary>
        /// 从类型成员获取指定Attribute特性
        /// </summary>
        /// <typeparam name="T">Attribute特性类型</typeparam>
        /// <param name="memberInfo">类型类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>存在返回第一个，不存在返回null</returns>
        public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            var descripts = memberInfo.GetCustomAttributes(typeof(T), inherit);
            return descripts.FirstOrDefault() as T;
        }

        /// <summary>
        /// 从类型成员获取指定Attribute特性
        /// </summary>
        /// <typeparam name="T">Attribute特性类型</typeparam>
        /// <param name="memberInfo">类型类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>返回所有指定Attribute特性的数组</returns>
        public static T[] GetAttributes<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }

        /// <summary> 属性名(Naming属性) </summary>
        /// <param name="item"></param>
        /// <param name="namingType"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string PropName(this MemberInfo item, NamingType? namingType = null, string def = null)
        {
            var propAttr = item.GetCustomAttribute<NamingAttribute>(true);
            if (propAttr != null)
            {
                if (propAttr.Ignore)
                    return string.Empty;
                if (!string.IsNullOrWhiteSpace(propAttr.Name))
                    return propAttr.Name;
                namingType = namingType ?? propAttr.NamingType;
            }

            var name = string.IsNullOrWhiteSpace(def) ? item.Name : def;
            switch (namingType)
            {
                case NamingType.CamelCase:
                    return name.ToCamelCase();
                case NamingType.UrlCase:
                    return name.ToUrlCase();
                default:
                    return name;
            }
        }

        /// <summary> 判断类型是否为集合类型 </summary>
        /// <param name="type">要处理的类型</param>
        /// <returns>是返回True，不是返回False</returns>
        public static bool IsEnumerable(this Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary> 判断当前泛型类型是否可由指定类型的实例填充 </summary>
        /// <param name="genericType">泛型类型</param>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        public static bool IsGenericAssignableFrom(this Type genericType, Type type)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("该功能只支持泛型类型的调用，非泛型类型可使用 IsAssignableFrom 方法。");
            }
            var allOthers = new List<Type> { type };
            if (genericType.IsInterface)
            {
                allOthers.AddRange(type.GetInterfaces());
            }

            foreach (var other in allOthers)
            {
                var cur = other;
                while (cur != null)
                {
                    if (cur.IsGenericType)
                    {
                        cur = cur.GetGenericTypeDefinition();
                    }
                    if (cur.IsSubclassOf(genericType) || cur == genericType)
                    {
                        return true;
                    }
                    cur = cur.BaseType;
                }
            }
            return false;
        }

        /// <summary> 程序集key </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string AssemblyKey(this Assembly assembly)
        {
            var assName = assembly.GetName();
            return $"{assName.Name}_{assName.Version}";
        }

        /// <summary> 默认值 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object DefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary> 属性检测 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">原对象(一般为数据库Entity)</param>
        /// <param name="target">目标对象(一般为Dto)</param>
        /// <param name="keyColumn">主键(不区分大小写)</param>
        /// <param name="reset">是否重置(即target有属性，但是属性的值为默认值时，是否认为有更新)</param>
        /// <param name="propAction">属性值更新操作(三个参数,SourcePropType,BeforeValue,AfterValue)</param>
        /// <returns></returns>
        public static PropertyInfo[] CheckProps<T>(this T source, object target, string keyColumn = "id",
            bool reset = false, Action<PropertyInfo, object, object> propAction = null)
        {
            if (source == null || target == null)
                return new PropertyInfo[] { };
            var sourceType = source.GetType();
            var targetType = target.GetType();
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = new List<PropertyInfo>();
            foreach (var sourceProp in sourceProps)
            {
                if (!string.IsNullOrWhiteSpace(keyColumn) && sourceProp.Name.EqualsIgnoreCase(keyColumn))
                    continue;
                //var targetProp = targetType.GetProperty(sourceProp.Name,
                //    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var targetProp = targetProps.FirstOrDefault(t => t.Name.EqualsIgnoreCase(sourceProp.Name));
                if (targetProp == null)
                    continue;
                //获取默认值
                var defValue = sourceProp.PropertyType.DefaultValue();
                var sourceValue = sourceProp.GetValue(source).CastTo(targetProp.PropertyType);
                var targetValue = targetProp.GetValue(target);

                //dto为默认值
                if (targetValue == null || targetValue.Equals(defValue))
                {
                    if (!reset || Equals(sourceValue, targetValue))
                        continue;
                    propAction?.Invoke(sourceProp, sourceValue, targetValue);
                    list.Add(sourceProp);
                }
                else if (!sourceValue.Equals(targetValue))
                {
                    propAction?.Invoke(sourceProp, sourceValue, targetValue);
                    list.Add(sourceProp);
                }
            }

            return list.ToArray();
        }

        /// <summary> 检查属性值变化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">原对象(一般为数据库Entity)</param>
        /// <param name="target">目标对象(一般为Dto)</param>
        /// <param name="keyColumn">主键(不区分大小写)</param>
        /// <param name="reset">是否重置(即target有属性，但是属性的值为默认值时，是否认为有更新)</param>
        /// <param name="setValue">是否更新Source属性值</param>
        /// <returns></returns>
        public static PropertyInfo[] CheckProps<T>(this T source, object target, string keyColumn = "id",
            bool reset = false, bool setValue = false)
        {
            return source.CheckProps(target, keyColumn, reset, (prop, before, after) =>
            {
                if (!setValue || !prop.CanWrite)
                    return;
                prop.SetValue(source, after);
            });
        }
    }
}
