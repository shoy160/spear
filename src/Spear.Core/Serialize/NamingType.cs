namespace Spear.Core.Serialize
{
    /// <summary> 命名规范 </summary>
    public enum NamingType
    {
        /// <summary> 默认命名,UserName </summary>
        Normal = 0,

        /// <summary> 驼峰命名,如：userName </summary>
        CamelCase = 1,

        /// <summary> url命名,如：user_name，注：反序列化时也需要指明 </summary>
        UrlCase = 2
    }
}
