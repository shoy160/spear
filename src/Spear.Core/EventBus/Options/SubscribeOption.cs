using System;

namespace Spear.Core.EventBus.Options
{
    /// <summary> 订阅选项 </summary>
    public class SubscribeOption
    {
        /// <summary> 是否开启死信队列 </summary>
        public bool EnableXDead { get; set; } = true;

        /// <summary> 是否开启重试机制 </summary>
        public bool EnableRetry { get; set; } = true;

        /// <summary> 重试间隔 </summary>
        public TimeSpan[] Times { get; set; }

        /// <summary> 持久化 </summary>
        public bool Durable { get; set; } = true;
    }
}
