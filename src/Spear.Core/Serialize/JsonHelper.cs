using System.Collections.Generic;

namespace Spear.Core.Serialize
{
    /// <summary> Json序列化辅助 </summary>
    public static class JsonHelper
    {
        private static IJsonHelper _jsonHelper;

        static JsonHelper()
        {
            _jsonHelper = new DefaultJsonHelper();
        }

        /// <summary> 使用某个Json序列化方式 </summary>
        /// <param name="helper"></param>
        public static void UseHelper(IJsonHelper helper)
        {
            _jsonHelper = helper;
        }

        /// <summary> Json序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <returns></returns>
        public static string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false)
        {
            return _jsonHelper.ToJson(jsonObj, namingType, indented);
        }

        /// <summary> Json序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <param name="retain">保留/排除</param>
        /// <param name="props">属性选择</param>
        /// <returns></returns>
        public static string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false,
            bool retain = true, params string[] props)
        {
            return _jsonHelper.ToJson(jsonObj, namingType, indented, retain, props);
        }

        /// <summary> Json反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public static T Json<T>(string json, NamingType namingType = NamingType.Normal)
        {
            return _jsonHelper.Json<T>(json, namingType);
        }

        /// <summary> Json列表反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public static IEnumerable<T> JsonList<T>(string json, NamingType namingType = NamingType.Normal)
        {
            return _jsonHelper.JsonList<T>(json, namingType);
        }

        /// <summary> 匿名对象反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="anonymousTypeObject"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        public static T Json<T>(string json, T anonymousTypeObject, NamingType namingType = NamingType.Normal)
        {
            return _jsonHelper.Json(json, anonymousTypeObject, namingType);
        }
    }
}
