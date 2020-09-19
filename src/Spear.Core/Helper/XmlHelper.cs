using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Spear.Core.Helper
{
    public class XmlHelper
    {
        private static ILogger Logger => CurrentIocManager.CreateLogger<XmlHelper>();
        private static void XmlSerializeInternal(Stream stream, object obj, Encoding encoding)
        {
            if (obj == null || encoding == null)
                return;
            try
            {
                var serializer = new XmlSerializer(obj.GetType());

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineChars = "\r\n",
                    Encoding = encoding,
                    IndentChars = "    "
                };

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, obj);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string XmlSerialize(object obj, Encoding encoding)
        {
            using (var stream = new MemoryStream())
            {
                XmlSerializeInternal(stream, obj, encoding);
                stream.Position = 0;
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEndAsync().SyncRun();
                }
            }
        }

        /// <summary>
        /// 将一个对象按XML序列化的方式写入到一个文件
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <param name="encoding">编码方式</param>
        public static void XmlSerializeToFile(object obj, string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
                return;
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializeInternal(file, obj, encoding);
            }
        }

        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="xml">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="def"></param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(string xml, Encoding encoding = null, T def = default)
        {
            if (string.IsNullOrEmpty(xml))
                return def;
            encoding = encoding ?? Encoding.UTF8;
            try
            {
                var mySerializer = new XmlSerializer(typeof(T));
                using (var ms = new MemoryStream(encoding.GetBytes(xml)))
                {
                    using (var sr = new StreamReader(ms, encoding))
                    {
                        return (T)mySerializer.Deserialize(sr);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return def;
            }
        }

        /// <summary>
        /// 读入一个文件，并按XML的方式反序列化对象。
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="def"></param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserializeFromPath<T>(string path, Encoding encoding = null, T def = default)
        {
            if (string.IsNullOrEmpty(path))
                return def;
            encoding = encoding ?? Encoding.UTF8;
            var xml = File.ReadAllText(path, encoding);
            return XmlDeserialize<T>(xml, encoding);
        }


        /// <summary>
        /// xml序列化
        /// </summary>
        /// <param name="path">xml文件路径</param>
        /// <param name="obj">序列化对象</param>
        /// <returns></returns>
        public static bool XmlSerializeFromPath(string path, object obj)
        {
            if (obj == null) return false;
            var directory = Path.GetDirectoryName(path);
            if (directory == null || !Directory.Exists(directory))
                return false;
            StreamWriter sw = null;
            try
            {
                var serializer = new XmlSerializer(obj.GetType());
                sw = new StreamWriter(path, false);
                serializer.Serialize(sw, obj);
                sw.Flush();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }
            finally
            {
                sw?.Close();
            }
        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <typeparam name="T">xml序列化类型</typeparam>
        /// <param name="path">xml文件路径</param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T XmlDeserializeFromPath<T>(string path, T def = default)
        {
            if (!File.Exists(path))
                return def;
            StreamReader fs = null;
            try
            {
                fs = new StreamReader(path, false);
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(fs).CastTo<T>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return def;
            }
            finally
            {
                fs?.Close();
            }
        }
    }
}
