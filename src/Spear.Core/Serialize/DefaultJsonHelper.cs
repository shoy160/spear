using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Spear.Core.Serialize
{
    /// <summary> 默认Json序列化 </summary>
    internal class DefaultJsonHelper : IJsonHelper
    {
        private const string DateFormatString = "yyyy-MM-dd HH:mm:ss";

        private static JsonSerializerSettings LoadSetting(NamingType namingType, bool indented)
        {
            var setting = new JsonSerializerSettings
            {
                ContractResolver = new JsonContractResolver(namingType)
            };
            if (indented)
                setting.Formatting = Formatting.Indented;
            setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            setting.DateFormatString = DateFormatString;
            setting.NullValueHandling = NullValueHandling.Ignore;
            return setting;
        }

        private static JsonSerializerSettings LoadSetting(NamingType namingType, bool indented, string[] props,
            bool retain)
        {
            var setting = LoadSetting(namingType, indented);
            if (props != null && props.Length > 0)
                setting.ContractResolver = new JsonContractResolver(namingType, retain, null, props);
            return setting;
        }

        /// <summary> 序列化为json格式 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <returns></returns>
        public string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false)
        {
            return JsonConvert.SerializeObject(jsonObj, LoadSetting(namingType, indented));
        }

        /// <summary> 序列化为json格式 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <param name="retain">保留/排除</param>
        /// <param name="props">属性选择</param>
        /// <returns></returns>
        public string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false,
            bool retain = true, params string[] props)
        {
            return JsonConvert.SerializeObject(jsonObj, LoadSetting(namingType, indented, props, retain));
        }

        /// <summary> 将json发序列化为对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public T Json<T>(string json, NamingType namingType = NamingType.Normal)
        {
            return JsonConvert.DeserializeObject<T>(json, LoadSetting(namingType, false));
        }

        /// <summary> 反序列化json为列表 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public IEnumerable<T> JsonList<T>(string json, NamingType namingType = NamingType.Normal)
        {
            var serializer = new JsonSerializer();
            if (namingType != NamingType.Normal)
                serializer.ContractResolver = new JsonContractResolver(namingType);
            using (var sr = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(sr))
                {
                    var obj = serializer.Deserialize(jsonReader, typeof(IEnumerable<T>));
                    return obj as IEnumerable<T>;
                }
            }
        }

        /// <summary> 反序列化到匿名对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="anonymousTypeObject"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public T Json<T>(string json, T anonymousTypeObject, NamingType namingType = NamingType.Normal)
        {
            return JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject, LoadSetting(namingType, false));
        }
    }
}
