using System;
using System.Collections.Generic;

namespace Spear.Core.EventBus.Options
{
    /// <summary> 发布选项 </summary>
    public class PublishOption
    {
        /// <summary> 延时发送 </summary>
        public TimeSpan? Delay { get; set; }

        /// <summary> 头 </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary> 持久化 </summary>
        public bool Durable { get; set; } = true;
    }
}
