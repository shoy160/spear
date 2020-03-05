using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spear.Core.Config;

namespace Spear.Core
{
    public static class SpearExtensions
    {
        private const string IpRegex = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

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

        public static bool IsIp(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) && Regex.IsMatch(str, IpRegex);
        }

        /// <summary> 服务命名 </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string ServiceName(this Assembly assembly)
        {
            var assName = assembly.GetName();
            return $"{assName.Name}_v{assName.Version.Major}";
        }

        /// <summary> 随机排序 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomSort<T>(this IEnumerable<T> array)
        {
            return array.OrderBy(t => Guid.NewGuid());
        }

        /// <summary> 异常信息格式化 </summary>
        /// <param name="ex"></param>
        /// <param name="isHideStackTrace"></param>
        /// <returns></returns>
        public static string Format(this Exception ex, bool isHideStackTrace = false)
        {
            var sb = new StringBuilder();
            var count = 0;
            var appString = string.Empty;
            while (ex != null)
            {
                if (count > 0)
                {
                    appString += "  ";
                }
                sb.AppendLine($"{appString}异常消息：{ex.Message}");
                sb.AppendLine($"{appString}异常类型：{ex.GetType().FullName}");
                sb.AppendLine($"{appString}异常方法：{(ex.TargetSite == null ? null : ex.TargetSite.Name)}");
                sb.AppendLine($"{appString}异常源：{ex.Source}");
                if (!isHideStackTrace && ex.StackTrace != null)
                {
                    sb.AppendLine($"{appString}异常堆栈：{ex.StackTrace}");
                }
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"{appString}内部异常：");
                    count++;
                }
                ex = ex.InnerException;
            }
            return sb.ToString();
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

        /// <summary>
        /// 对象转换为泛型
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastTo<T>(this object obj)
        {
            return obj.CastTo(default(T));
        }

        /// <summary>
        /// 对象转换为泛型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="def"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastTo<T>(this object obj, T def)
        {
            var value = obj.CastTo(typeof(T));
            if (value == null)
                return def;
            return (T)value;
        }

        /// <summary> 把对象类型转换为指定类型 </summary>
        /// <param name="obj"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object CastTo(this object obj, Type conversionType)
        {
            if (obj == null)
            {
                return conversionType.IsGenericType ? Activator.CreateInstance(conversionType) : null;
            }
            if (conversionType.IsNullableType())
                conversionType = conversionType.GetUnNullableType();
            try
            {
                if (conversionType == obj.GetType())
                    return obj;
                if (conversionType.IsEnum)
                {
                    return obj is string s
                        ? Enum.Parse(conversionType, s)
                        : Enum.ToObject(conversionType, obj);
                }

                if (!conversionType.IsInterface && conversionType.IsGenericType)
                {
                    var innerType = conversionType.GetGenericArguments()[0];
                    var innerValue = CastTo(obj, innerType);
                    return Activator.CreateInstance(conversionType, innerValue);
                }

                if (conversionType == typeof(Guid))
                {
                    if (Guid.TryParse(obj.ToString(), out var guid))
                        return guid;
                    return null;
                }

                if (conversionType == typeof(Version))
                {
                    if (Version.TryParse(obj.ToString(), out var version))
                        return version;
                    return null;
                }

                return !(obj is IConvertible) ? obj : Convert.ChangeType(obj, conversionType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary> 获取环境变量 </summary>
        /// <param name="name">变量名称</param>
        /// <param name="target">存储目标</param>
        /// <returns></returns>
        public static string Env(this string name, EnvironmentVariableTarget? target = null)
        {
            return target.HasValue
                ? Environment.GetEnvironmentVariable(name, target.Value)
                : Environment.GetEnvironmentVariable(name);
        }

        /// <summary> 获取环境变量 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">变量名称</param>
        /// <param name="def">默认值</param>
        /// <param name="target">存储目标</param>
        /// <returns></returns>
        public static T Env<T>(this string name, T def = default(T), EnvironmentVariableTarget? target = null)
        {
            var env = name.Env(target);
            return string.IsNullOrWhiteSpace(env) ? def : env.CastTo(def);
        }

        /// <summary> 读取配置文件 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configName"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T Config<T>(this string configName, T def = default)
        {
            var helper = ConfigManager.Instance;
            return helper.Get(configName, def);
        }

        public static T Const<T>(this string key, T def = default)
        {
            return string.IsNullOrWhiteSpace(key) ? def : $"const:{key}".Config(def);
        }

        public static string Site(this string site)
        {
            return string.IsNullOrWhiteSpace(site) ? string.Empty : $"sites:{site}".Config<string>();
        }

        public static string Site(this Enum site)
        {
            return $"sites:{site}".Config<string>();
        }

        /// <summary> 使用本地文件配置 </summary>
        public static void UseLocal(this ConfigManager manager, string configDir)
        {
            if (string.IsNullOrWhiteSpace(configDir))
                return;
            configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configDir);
            if (!Directory.Exists(configDir))
                return;
            var jsons = Directory.GetFiles(configDir, "*.json");
            if (jsons.Any())
            {
                manager.Build(b =>
                {
                    foreach (var json in jsons)
                    {
                        b.AddJsonFile(json, false, true);
                    }
                });
            }
        }

        /// <summary> 获取值 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T GetValue<T>(this IDictionary<object, object> dict, object key, T def = default)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
            {
                return value.CastTo(def);
            }
            return def;
        }

        /// <summary> 将网关参数转为类型 </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static T ToObject<T>(this IDictionary<object, object> dict)
        {
            var type = typeof(T);
            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();

            foreach (var item in properties)
            {
                var key = item.Name;
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                var value = dict.GetValue<object>(key);
                if (value != null)
                {
                    item.SetValue(obj, value.CastTo(item.PropertyType));
                }
            }

            return obj.CastTo<T>();
        }

        /// <summary> zip压缩 </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static async Task<byte[]> Zip(this byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return buffer;
            using (var stream = new MemoryStream())
            {
                using (var zip = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    await zip.WriteAsync(buffer, 0, buffer.Length);
                }
                return stream.ToArray();
            }
        }

        /// <summary> zip解压 </summary>
        /// <param name="zipBuffer"></param>
        /// <returns></returns>
        public static async Task<byte[]> UnZip(this byte[] zipBuffer)
        {
            if (zipBuffer == null || zipBuffer.Length == 0)
                return zipBuffer;

            using (var zipStream = new MemoryStream(zipBuffer))
            {
                using (var zip = new GZipStream(zipStream, CompressionMode.Decompress))
                {
                    using (var stream = new MemoryStream())
                    {
                        var buffer = new byte[2048];
                        while (true)
                        {
                            var count = await zip.ReadAsync(buffer, 0, buffer.Length);
                            if (count == 0) break;
                            await stream.WriteAsync(buffer, 0, count);
                        }
                        return stream.ToArray();
                    }
                }
            }
        }

        public static string TypeName(this Type type)
        {
            var code = Type.GetTypeCode(type);
            if (code != TypeCode.Object && type.BaseType != typeof(Enum))
                return type.FullName;
            return type.AssemblyQualifiedName;
        }
    }
}
