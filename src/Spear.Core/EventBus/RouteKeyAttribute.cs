using System;

namespace Spear.Core.EventBus
{
    /// <inheritdoc />
    /// <summary> 事件路由键 </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RouteKeyAttribute : Attribute
    {
        public RouteKeyAttribute(string key)
        {
            Key = key;
        }
        /// <summary> 路由键 </summary>
        public string Key { get; set; }
    }
}
