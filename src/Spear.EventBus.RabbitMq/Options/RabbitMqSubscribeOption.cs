using Spear.Core.EventBus.Options;
using System;

namespace Spear.RabbitMq.Options
{
    /// <summary> 订阅属性 </summary>
    public class RabbitMqSubscribeOption : SubscribeOption
    {
        /// <summary> 队列名称 </summary>
        public string Queue { get; set; }
        /// <summary> 路由键 </summary>
        public string RouteKey { get; set; }

        /// <summary> 交换机 </summary>
        public string Exchange { get; set; }

        /// <summary> 交换机类型 </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;

        /// <summary> 死信队列交换机 </summary>
        public string XDeadExchange { get; set; }

        /// <summary> 死信队列路由键 </summary>
        public string XDeadRouteKey { get; set; }

        /// <summary> 同时接收的消息数 </summary>
        public int PrefetchCount { get; set; } = -1;

        /// <summary> 构造函数 </summary>
        public RabbitMqSubscribeOption()
        {
            Times = new[]
            {
                TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10),
                TimeSpan.FromHours(1), TimeSpan.FromHours(2), TimeSpan.FromHours(6), TimeSpan.FromHours(12)
            };
        }

        /// <inheritdoc />
        /// <summary> 构造函数 </summary>
        /// <param name="queue"></param>
        /// <param name="routeKey"></param>
        public RabbitMqSubscribeOption(string queue, string routeKey = null) : this()
        {
            Queue = queue;
            RouteKey = routeKey;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Queue);
    }
}
