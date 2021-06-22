using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spear.Core.Dependency;
using Spear.Core.EventBus;
using Spear.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spear.RabbitMq
{
    public class RabbitMessageQueue : IMessageQueue
    {
        private readonly IRabbitMqConnection _connection;
        private readonly Encoding _encoding;
        private readonly ILogger _logger;
        private readonly string _queue;

        public RabbitMessageQueue(IRabbitMqConnection connection, string queue)
        {
            _connection = connection;
            _encoding = Encoding.UTF8;
            _logger = CurrentIocManager.CreateLogger<RabbitMessageQueue>();
            _queue = queue;
            Channel.ExchangeDeclare(_connection.Broker, ExchangeType.Direct, false, true);
            Channel.QueueDeclare(_queue, true, false, false);
            Channel.QueueBind(_queue, _connection.Broker, _queue);
        }

        private IModel _channel;
        public IModel Channel
        {
            get
            {
                if (_channel != null && _channel.IsOpen)
                    return _channel;
                if (!_connection.IsConnected)
                    _connection.TryConnect();
                return _channel = _connection.CreateModel();
            }
        }


        public Task Send<T>(T message, IDictionary<string, object> headers = null)
        {
            var body = _encoding.GetBytes(JsonConvert.SerializeObject(message));
            return Send(body, headers);
        }

        public Task Send(byte[] message, IDictionary<string, object> headers = null)
        {
            //持久化处理
            var properties = Channel.CreateBasicProperties();
            if (headers != null && headers.Keys.Count > 0)
                properties.Headers = headers;

            properties.Persistent = true;
            properties.ContentEncoding = _encoding.EncodingName;

            Channel.BasicPublish(_connection.Broker, _queue, properties, message);
            return Task.CompletedTask;
        }

        public Task<T> Receive<T>()
        {
            var res = Channel.BasicGet(_queue, false);

            try
            {
                if (res == null)
                    return Task.FromResult(default(T));
                var model = JsonConvert.DeserializeObject<T>(_encoding.GetString(res.Body.ToArray()));
                Channel.BasicAck(res.DeliveryTag, false);
                return Task.FromResult(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Task.FromResult(default(T));
            }
        }

        public async Task<IEnumerable<T>> Receive<T>(int size)
        {
            var def = default(T);
            var list = new List<T>();
            for (var i = 0; i < size; i++)
            {
                var item = await Receive<T>();
                if (item == null || item.Equals(def))
                    break;
                list.Add(item);
            }
            return list;
        }

        public Task Subscibe<T>(Action<T> action)
        {
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    var message = JsonConvert.DeserializeObject<T>(_encoding.GetString(ea.Body.ToArray()));
                    action(message);
                    Channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    if (ex is BusiException)
                    {
                        Channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }
                    _logger.LogError(ex, ex.Message);
                    Channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            Channel.BasicQos(0, 1, false);
            Channel.BasicConsume(_queue, false, consumer);
            return Task.CompletedTask;
        }
    }
}
