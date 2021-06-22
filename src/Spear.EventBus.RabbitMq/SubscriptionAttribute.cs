using Spear.RabbitMq.Options;
using System;
using System.Linq;

namespace Spear.RabbitMq
{
    /// <inheritdoc />
    /// <summary> 订阅属性 </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscriptionAttribute : Attribute
    {
        /// <summary> 订阅配置 </summary>
        public RabbitMqSubscribeOption Option { get; set; }

        /// <summary> 订阅属性 </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="routeKey">路由键,默认为事件属性的RouteKey</param>
        /// <param name="fetchCount">每次处理数量,小于等于0不限制，默认1</param>
        /// <param name="retry">是否开启重试</param>
        /// <param name="times">秒</param>
        public SubscriptionAttribute(string queue, string routeKey = null, int fetchCount = 1, bool retry = true,
            int[] times = null)
        {
            Option = new RabbitMqSubscribeOption(queue, routeKey)
            {
                PrefetchCount = fetchCount,
                EnableRetry = retry
            };
            if (times != null && times.Any())
                Option.Times = times.Select(t => TimeSpan.FromSeconds(t)).ToArray();

            Queue = queue;
            RouteKey = routeKey;
        }

        /// <summary> 队列名称 </summary>
        public string Queue
        {
            get => Option.Queue;
            set => Option.Queue = value;
        }

        /// <summary> 路由键 </summary>
        public string RouteKey
        {
            get => Option.RouteKey;
            set => Option.RouteKey = value;
        }
    }
}
