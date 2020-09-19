using Spear.Core.EventBus.Options;
using Spear.Core.Message;
using System;
using System.Threading.Tasks;

namespace Spear.Core.EventBus
{
    public abstract class AbstractEventBus : IEventBus
    {
        /// <summary> 订阅管理器 </summary>
        protected readonly ISubscribeManager SubscriptionManager;

        /// <summary> 名称 </summary>
        public string Name { get; }

        /// <summary> 编解码器 </summary>
        public IMessageCodec Codec { get; }

        protected AbstractEventBus(ISubscribeManager manager, IMessageCodec messageCodec, string name = null)
        {
            SubscriptionManager = manager ?? new DefaultSubscribeManager();
            Codec = messageCodec;
            Name = name;
        }

        public abstract Task Subscribe<T, TH>(Func<TH> handler, SubscribeOption option = null)
            where TH : IEventHandler<T>;

        public Task Unsubscribe<T, TH>() where TH : IEventHandler<T>
        {
            SubscriptionManager.RemoveSubscription<T, TH>();
            return Task.CompletedTask;
        }

        public Task Publish(string key, object message, PublishOption option = null)
        {
            var data = Codec.Encode(message);
            return Publish(key, data, option);
        }

        public abstract Task Publish(string key, byte[] message, PublishOption option = null);
    }
}
