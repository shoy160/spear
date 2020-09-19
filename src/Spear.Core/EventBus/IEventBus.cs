using Spear.Core.EventBus.Options;
using Spear.Core.Message;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.EventBus
{
    /// <summary> 事件总线 </summary>
    public interface IEventBus
    {
        string Name { get; }
        /// <summary> 编解码器 </summary>
        IMessageCodec Codec { get; }

        /// <summary> 订阅 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler"></param>
        /// <param name="option"></param>
        Task Subscribe<T, TH>(Func<TH> handler, SubscribeOption option = null)
            where TH : IEventHandler<T>;

        /// <summary> 取消订阅 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        Task Unsubscribe<T, TH>()
            where TH : IEventHandler<T>;

        /// <summary> 发布 </summary>
        /// <param name="key">事件</param>
        /// <param name="message"></param>
        /// <param name="option"></param>
        Task Publish(string key, object message, PublishOption option = null);
    }

    /// <summary> 扩展 </summary>
    public static class EventBusExtensions
    {
        /// <summary> 获取事件的路由键 </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public static string GetRouteKey(this MemberInfo eventType)
        {
            var attr = eventType.GetCustomAttribute<RouteKeyAttribute>();
            return attr == null ? eventType.Name : attr.Key;
        }

        /// <summary> 发布 </summary>
        /// <param name="eventBus"></param>
        /// <param name="event">事件</param>
        /// <param name="option"></param>
        public static Task Publish(this IEventBus eventBus, DEvent @event, PublishOption option = null)
        {
            var key = @event.GetType().GetRouteKey();
            return eventBus.Publish(key, @event, option);
        }

        /// <summary> 延时发布 </summary>
        /// <param name="key">事件</param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        public static Task Publish(this IEventBus eventBus, string key, object message, TimeSpan delay)
        {
            return eventBus.Publish(key, message, new PublishOption { Delay = delay });
        }

        /// <summary> 发布 </summary>
        /// <param name="eventBus"></param>
        /// <param name="event">事件</param>
        /// <param name="option"></param>
        /// <param name="delay"></param>
        public static Task Publish(this IEventBus eventBus, DEvent @event, TimeSpan delay)
        {
            var key = @event.GetType().GetRouteKey();
            return eventBus.Publish(key, @event, new PublishOption { Delay = delay });
        }
    }
}
