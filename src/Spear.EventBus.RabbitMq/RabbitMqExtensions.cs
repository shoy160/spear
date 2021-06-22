using Spear.RabbitMq.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Spear.RabbitMq
{
    public static class RabbitMqExtensions
    {
        /// <summary> 申明队列并设置死信队列 </summary>
        /// <param name="channel"></param>
        /// <param name="queue"></param>
        /// <param name="exchange"></param>
        /// <param name="routeKey"></param>
        /// <param name="option"></param>
        public static void DeclareWithDlx(this IModel channel, string queue, string exchange, string routeKey, RabbitMqSubscribeOption option)
        {
            var xRouteKey = string.IsNullOrWhiteSpace(option.XDeadRouteKey) ? $"~dlx_{queue}" : option.XDeadRouteKey;
            var xExchange = string.IsNullOrWhiteSpace(option.XDeadExchange) ? exchange : option.XDeadExchange;
            var args = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", xExchange},
                {"x-dead-letter-routing-key", xRouteKey}
            };
            //死信队列
            //1.消息被拒绝，并且设置ReQueue参数false；
            //2.消息过期；
            //3.队列打到最大长度；
            channel.QueueDeclare(xRouteKey, true, false);
            channel.QueueBind(xRouteKey, xExchange, xRouteKey, null);

            //声明队列
            channel.QueueDeclare(queue, true, false, false, args);
            channel.QueueBind(queue, xExchange, routeKey);
        }

        /// <summary> 延迟发布 </summary>
        /// <param name="channel"></param>
        /// <param name="exchange"></param>
        /// <param name="routeKey"></param>
        /// <param name="body"></param>
        /// <param name="prop"></param>
        /// <param name="delay"></param>
        public static void DelayPublish(this IModel channel, string exchange, string routeKey, byte[] body, TimeSpan delay,
            IBasicProperties prop = null)
        {
            //延迟队列
            var delayQueue = $"~delay_{routeKey}";
            var args = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", exchange},
                {"x-dead-letter-routing-key", routeKey}
            };
            //死信队列
            //1.消息被拒绝，并且设置ReQueue参数false；
            //2.消息过期；
            //3.队列打到最大长度；
            channel.QueueDeclare(delayQueue, true, false, true, args);
            channel.QueueBind(delayQueue, exchange, delayQueue, null);
            if (prop == null)
                prop = channel.CreateBasicProperties();
            prop.Expiration = ((long)delay.TotalMilliseconds).ToString();
            channel.BasicPublish(exchange, delayQueue, prop, body);
        }
    }
}
