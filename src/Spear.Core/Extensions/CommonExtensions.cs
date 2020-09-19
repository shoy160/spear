using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spear.Core.Extensions
{
    /// <summary> 对象扩展辅助 </summary>
    public static class CommonExtensions
    {
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

        /// <summary>
        /// 将对象[主要是匿名对象]转换为dynamic
        /// </summary>
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            var type = value.GetType();
            var properties = TypeDescriptor.GetProperties(type);
            foreach (PropertyDescriptor property in properties)
            {
                var val = property.GetValue(value);
                if (property.PropertyType.FullName != null && property.PropertyType.FullName.StartsWith("<>f__AnonymousType"))
                {
                    var dval = val.ToDynamic();
                    expando.Add(property.Name, dval);
                }
                else
                {
                    expando.Add(property.Name, val);
                }
            }

            return (ExpandoObject)expando;
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

        /// <summary> 获取最初的异常信息 </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Exception GetInnermostException(this Exception exception)
        {
            if (exception == null)
                return null;
            var inner = exception;
            while (inner.InnerException != null)
                inner = inner.InnerException;
            return inner;
        }

        /// <summary> unescape </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UnEscape(this object value)
        {
            if (value == null)
                return string.Empty;
            var type = value.GetType();
            //枚举值
            if (type.IsEnum)
                return value.CastTo(0).ToString();
            //布尔值
            if (type == typeof(bool))
                return ((bool)value ? 1 : 0).ToString();

            var sb = new StringBuilder();
            var str = value.ToString();
            var len = str.Length;
            var i = 0;
            while (i != len)
            {
                sb.Append(Uri.IsHexEncoding(str, i) ? Uri.HexUnescape(str, ref i) : str[i++]);
            }

            return sb.ToString();
        }

        public static object GetService(this IServiceProvider provider, Type type, string name)
        {
            var services = provider.GetServices(type);
            return services.First(t => t.GetType().PropName() == name);
        }

        public static T GetService<T>(this IServiceProvider provider, string name)
        {
            var services = provider.GetServices<T>();
            return services.First(t => t.GetType().PropName() == name);
        }

        ///// <summary> 模型验证 </summary>
        ///// <param name="obj"></param>
        ///// <param name="items"></param>
        //public static void Validate(this object obj, Dictionary<object, object> items = null)
        //{
        //    if (obj == null) return;
        //    var validationContext = new ValidationContext(obj, items);
        //    var results = new List<ValidationResult>();
        //    var isValid = Validator.TryValidateObject(obj, validationContext, results, true);

        //    if (isValid) return;
        //    var error = results.First();
        //    throw new ArgumentNullException(error.MemberNames.FirstOrDefault(), error.ErrorMessage);
        //}

        /// <summary> 获取字符串结果 </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringResult(this object value)
        {
            if (value == null) return string.Empty;
            if (value.GetType().IsSimpleType())
            {
                if (value is DateTime date)
                    return date.ToString("yyyy-MM-dd HH:mm:ss");
                return value.ToString();
            }
            return JsonHelper.ToJson(value);
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

        /// <summary> 获取流对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="contentType"></param>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static async Task<T> ReadAsync<T>(this Stream stream, string contentType = null, bool gzip = false)
        {
            if (stream == null) return default;

            async Task<string> GetBody()
            {
                try
                {
                    if (!stream.CanRead)
                        return string.Empty;
                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);
                    string text;
                    if (gzip)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await stream.CopyToAsync(ms);
                            var buffer = await ms.GetBuffer().UnZip();
                            text = Encoding.UTF8.GetString(buffer);
                        }
                    }
                    else
                    {
                        var reader = new StreamReader(stream);
                        text = await reader.ReadToEndAsync();
                    }

                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);
                    return text;

                }
                catch
                {
                    return string.Empty;
                }
            }

            if (typeof(T).IsSimpleType())
                return (await GetBody()).CastTo<T>();
            string body;

            switch (contentType)
            {
                case "application/xml":
                case "text/xml":
                    body = await GetBody();
                    var dict = new Dictionary<string, object>();
                    dict.FromXml(body);
                    return dict.ToObject<T>();
                default:
                    body = await GetBody();
                    return JsonHelper.Json<T>(body, NamingType.CamelCase);
            }
        }


    }
}