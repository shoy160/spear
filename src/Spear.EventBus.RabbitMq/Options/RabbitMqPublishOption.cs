using Spear.Core.EventBus.Options;

namespace Spear.RabbitMq.Options
{
    /// <summary> 发布属性 </summary>
    public class RabbitMqPublishOption : PublishOption
    {
        /// <summary> 交换机 </summary>
        public string Exchange { get; set; }

        /// <summary> 交换机类型 </summary>
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;

        /// <summary> 订阅配置 </summary>
        public RabbitMqSubscribeOption Subscribe { get; set; }
    }
}
