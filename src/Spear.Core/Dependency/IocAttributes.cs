using System;

namespace Spear.Core.Dependency
{
    /// <summary> 配置注入 </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        /// <summary> 配置键 </summary>
        public string Key { get; }

        public ConfigAttribute(string key)
        {
            Key = key;
        }
    }

    /// <summary> 属性注入 </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AutowiredAttribute : Attribute { }
}
