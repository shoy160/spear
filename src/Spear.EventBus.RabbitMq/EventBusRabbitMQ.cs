using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Spear.Core.Dependency;
using Spear.Core.EventBus;
using Spear.Core.EventBus.Options;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Spear.Core.Message;
using Spear.Core.Serialize;
using Spear.RabbitMq.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMqSubscribeOption = Spear.RabbitMq.Options.RabbitMqSubscribeOption;
using Microsoft.Extensions.Logging;

namespace Spear.RabbitMq
{
    /// <summary> RabbitMQ事件总线 </summary>
    public class EventBusRabbitMq : AbstractEventBus, IDisposable
    {
        private readonly string _brokerName;

        private readonly IRabbitMqConnection _connection;
        private readonly ILogger _logger;

        private IModel _consumerChannel;
        private string _queueName;
        private const string DelayTimesKey = "delay_times";

        /// <summary> RabbitMQ事件总线 </summary>
        /// <param name="connection"></param>
        /// <param name="subsManager"></param>
        /// <param name="messageCodec"></param>
        public EventBusRabbitMq(IRabbitMqConnection connection, ISubscribeManager subsManager, IMessageCodec messageCodec)
            : base(subsManager, messageCodec, connection?.Name)
        {
            _connection =
                connection ?? throw new ArgumentNullException(nameof(connection));
            _brokerName = connection.Broker;
            _logger = CurrentIocManager.CreateLogger<EventBusRabbitMq>();
            SubscriptionManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        /// <summary> 获取订阅的队列信息 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static RabbitMqSubscribeOption GetSubscription(Type type)
        {
            var attr = type.GetCustomAttribute<SubscriptionAttribute>() ?? new SubscriptionAttribute(type.FullName);
            if (string.IsNullOrWhiteSpace(attr.Queue))
            {
                attr.Queue = type.FullName;
            }
            return attr.Option;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            using (var channel = _connection.CreateModel())
            {
                channel.QueueUnbind(_queueName, _brokerName, eventName);

                if (!SubscriptionManager.IsEmpty)
                    return;
                _queueName = string.Empty;
                _consumerChannel?.Close();
            }
        }

        /// <summary> 发布消息 </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public override async Task Publish(string key, byte[] message, PublishOption option = null)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            RabbitMqPublishOption opt;
            if (option == null)
                opt = new RabbitMqPublishOption();
            else if (option is RabbitMqPublishOption rbOpt)
                opt = rbOpt;
            else
                opt = new RabbitMqPublishOption { Delay = option.Delay, Durable = option.Durable, Headers = option.Headers };

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => { _logger.LogWarning(ex, ex.ToString()); });
            await policy.ExecuteAsync(() =>
            {
                using (var channel = _connection.CreateModel())
                {
                    var prop = channel.CreateBasicProperties();
                    prop.DeliveryMode = 2;
                    if (opt.Headers != null)
                    {
                        if (prop.Headers == null)
                            prop.Headers = new Dictionary<string, object>();
                        foreach (var header in opt.Headers)
                        {
                            prop.Headers.AddOrUpdate(header.Key, header.Value);
                        }
                    }
                    //声明Exchange
                    var exchange = string.IsNullOrWhiteSpace(opt.Exchange) ? _brokerName : opt.Exchange;
                    channel.ExchangeDeclare(exchange, opt.ExchangeType, opt.Durable);
                    if (opt.Subscribe != null && opt.Subscribe.IsValid)
                    {
                        DeclareAndBindQueue(opt.Subscribe.Queue, opt.Subscribe.RouteKey, opt.Subscribe);
                    }
                    if (opt.Delay.HasValue)
                    {
                        channel.DelayPublish(exchange, key, message, opt.Delay.Value, prop);
                    }
                    else
                    {
                        channel.BasicPublish(exchange, key, prop, message);
                    }
                }
                return Task.CompletedTask;
            });
        }

        /// <summary> 定义并绑定队列 </summary>
        /// <param name="queue"></param>
        /// <param name="key"></param>
        /// <param name="option"></param>
        private void DeclareAndBindQueue(string queue, string key, RabbitMqSubscribeOption option)
        {
            var exchange = string.IsNullOrWhiteSpace(option.Exchange) ? _brokerName : option.Exchange;
            _consumerChannel.ExchangeDeclare(exchange, option.ExchangeType, option.Durable);
            if (option.EnableXDead)
            {
                _consumerChannel.DeclareWithDlx(queue, exchange, key, option);
            }
        }

        private void RemoteLog(string queue, byte[] data, Exception ex)
        {
            //object @event;
            //try
            //{
            //    //远程日志
            //    var type = SubscriptionManager.GetEventTypeByName(queue);
            //    @event = Codec.Decode(data, type);
            //}
            //catch
            //{
            //    @event = null;
            //}

            //var msg = $"{queue},busi:{ex.Message}";
            //if (@event != null)
            //    msg += $",event:{JsonHelper.ToJson(@event)}";
            //CurrentIocManager.Resolve<IRemoteLogger>().Logger(msg, LogLevel.Warning, ex,
            //    logger: nameof(EventBusRabbitMq));
        }

        /// <summary> 接收消息 </summary>
        /// <param name="queue">队列</param>
        /// <param name="ea"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private async Task ReceiveMessage(string queue, BasicDeliverEventArgs ea, RabbitMqSubscribeOption option)
        {
            try
            {
                await SubscriptionManager.ProcessEvent(queue, ea.Body.ToArray());
                _consumerChannel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                //非业务异常,可重新入队
                if (ex.GetBaseException() is BusiException busi)
                {
                    //拒收，不重新入列
                    _consumerChannel.BasicNack(ea.DeliveryTag, false, false);
                    _logger.LogWarning($"{queue},busi:{busi.Message}");
                    RemoteLog(queue, ea.Body.ToArray(), busi);
                    return;
                }
                _logger.LogError(ex, ex.Message);

                if (!option.EnableRetry)
                {
                    _consumerChannel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                var maxTime = option.Times.Length;

                var times = 0;
                if (ea.BasicProperties.Headers != null &&
                    ea.BasicProperties.Headers.TryGetValue(DelayTimesKey, out var t))
                {
                    times = t.CastTo(0);
                }

                if (times >= maxTime)
                {
                    //拒收，不重新入列
                    _consumerChannel.BasicNack(ea.DeliveryTag, false, false);
                    _logger.LogWarning($"{queue},retry times > {maxTime}");
                    return;
                }

                //延时入列
                times++;
                var delay = option.Times[times - 1];
                _consumerChannel.BasicAck(ea.DeliveryTag, false);

                if (!_connection.IsConnected)
                    _connection.TryConnect();
                using (var channel = _connection.CreateModel())
                {
                    var prop = _consumerChannel.CreateBasicProperties();
                    prop.DeliveryMode = 2;
                    prop.Headers = prop.Headers ?? new Dictionary<string, object>();
                    prop.Headers.Add(DelayTimesKey, times);
                    //绑定队列路由
                    channel.QueueBind(queue, _brokerName, queue);

                    channel.DelayPublish(_brokerName, queue, ea.Body.ToArray(), delay, prop);
                }
            }
        }

        /// <summary> 订阅消息 </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        /// <param name="handler"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public override Task Subscribe<T, TH>(Func<TH> handler, SubscribeOption option = null)
        {
            _consumerChannel = _consumerChannel ?? CreateConsumerChannel();

            var opt = (option ?? GetSubscription(typeof(TH))) as RabbitMqSubscribeOption ??
                      new RabbitMqSubscribeOption();
            if (opt.PrefetchCount > 0)
            {
                //同时只能接受1条消息
                _consumerChannel.BasicQos(0, (ushort)opt.PrefetchCount, false);
            }

            var queue = opt.Queue;
            var dataType = typeof(T);
            string key;
            if (typeof(DEvent).IsAssignableFrom(dataType))
            {
                key = !string.IsNullOrWhiteSpace(opt.RouteKey)
                    ? opt.RouteKey
                    : typeof(T).GetRouteKey();
            }
            else
            {
                key = string.IsNullOrWhiteSpace(opt.RouteKey) ? opt.Queue : opt.RouteKey;
            }

            DeclareAndBindQueue(queue, key, opt);
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var consumer = new EventingBasicConsumer(_consumerChannel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    await ReceiveMessage(queue, ea, opt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"MQ订阅处理异常：{ex.Message}");
                }

            };

            _consumerChannel.BasicConsume(queue, false, consumer);

            SubscriptionManager.AddSubscription<T, TH>(handler, queue);
            return Task.CompletedTask;
        }

        /// <summary> 释放资源 </summary>
        public void Dispose()
        {
            _consumerChannel?.Dispose();
            _connection?.Dispose();
            SubscriptionManager.Clear();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var channel = _connection.CreateModel();

            ////同时只能接受1条消息
            //channel.BasicQos(0, 1, false);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };

            return channel;
        }
    }
}
