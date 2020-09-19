using System.Collections.Generic;

namespace Spear.Core.Serialize
{
    /// <summary> Json序列化接口 </summary>
    public interface IJsonHelper
    {
        /// <summary> Json序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <returns></returns>
        string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false);
        /// <summary> Json序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <param name="namingType">命名规则</param>
        /// <param name="indented">是否缩进</param>
        /// <param name="retain">保留/排除</param>
        /// <param name="props">属性选择</param>
        /// <returns></returns>
        string ToJson<T>(T jsonObj, NamingType namingType = NamingType.Normal, bool indented = false, bool retain = true, params string[] props);

        /// <summary> Json反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        T Json<T>(string json, NamingType namingType = NamingType.Normal);

        /// <summary> Json列表反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        IEnumerable<T> JsonList<T>(string json, NamingType namingType = NamingType.Normal);

        /// <summary> 匿名对象反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="anonymousTypeObject"></param>
        /// <param name="namingType"></param>
        /// <returns></returns>
        T Json<T>(string json, T anonymousTypeObject, NamingType namingType = NamingType.Normal);
    }
}
