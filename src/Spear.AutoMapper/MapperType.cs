namespace Spear.AutoMapper
{
    /// <summary> 转换类型 </summary>
    public enum MapperType
    {
        /// <summary> 普通 </summary>
        Normal,
        /// <summary> 从url命名到驼峰命名 </summary>
        FromUrl,
        /// <summary> 从驼峰命名url命名 </summary>
        ToUrl
    }
}
