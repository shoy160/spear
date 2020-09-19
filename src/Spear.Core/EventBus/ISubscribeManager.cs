using Spear.Core.Dependency;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spear.Core.Message;

namespace Spear.Core.EventBus
{
    /// <summary> 订阅管理器 </summary>
    public interface ISubscribeManager : ISingleDependency
    {
        /// <summary> 是否为空 </summary>
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        /// <summary> 添加订阅 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler"></param>
        /// <param name="eventKey"></param>
        void AddSubscription<T, TH>(Func<TH> handler, string eventKey = null)
            where TH : IEventHandler<T>;

        /// <summary> 删除订阅 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        void RemoveSubscription<T, TH>(string eventKey = null)
            where TH : IEventHandler<T>;
        /// <summary> 是否已订阅 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasSubscriptionsForEvent<T>();
        /// <summary> 是否已订阅 </summary>
        /// <param name="eventKey"></param>
        /// <returns></returns>
        bool HasSubscriptionsForEvent(string eventKey);
        /// <summary> 获取订阅 </summary>
        /// <param name="eventKey"></param>
        /// <returns></returns>
        Type GetEventTypeByName(string eventKey);
        /// <summary> 清空订阅 </summary>
        void Clear();
        /// <summary> 获取订阅事件 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<Delegate> GetHandlersForEvent<T>() where T : DEvent;
        /// <summary> 获取订阅事件 </summary>
        /// <param name="eventKey"></param>
        /// <returns></returns>
        IEnumerable<Delegate> GetHandlersForEvent(string eventKey);
    }

    public static class SubsciptionManagerExtension
    {
        /// <summary> 执行事务 </summary>
        /// <param name="manager"></param>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task ProcessEvent(this ISubscribeManager manager, string eventName, byte[] data)
        {
            var codec = CurrentIocManager.Resolve<IMessageCodec>();
            if (manager.HasSubscriptionsForEvent(eventName))
            {
                var eventType = manager.GetEventTypeByName(eventName);
                var @event = codec.Decode(data, eventType);
                var handlers = manager.GetHandlersForEvent(eventName);

                foreach (var handlerfactory in handlers)
                {
                    var handler = handlerfactory.DynamicInvoke();
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    var method = concreteType.GetMethod("Handle");
                    if (method == null)
                        continue;
                    await (Task)method.Invoke(handler, new[] { @event });
                }
            }
        }
    }
}
