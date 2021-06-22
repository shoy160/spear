using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Spear.Core.Dependency;
using System;
using System.IO;
using System.Net.Sockets;

namespace Spear.RabbitMq
{
    public class DefaultRabbitMqConnection : IRabbitMqConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        private IConnection _connection;
        private bool _disposed;

        private readonly object _syncRoot = new object();

        public DefaultRabbitMqConnection(string configName) : this(RabbitMqConfig.Config(configName))
        {
            Name = configName;
        }

        public DefaultRabbitMqConnection(RabbitMqConfig config = null)
        {
            config = config ?? RabbitMqConfig.Config();
            if (string.IsNullOrWhiteSpace(config?.Host))
                throw new ArgumentException(nameof(config));
            _connectionFactory = new ConnectionFactory { HostName = config.Host, Port = config.Port };
            if (!string.IsNullOrWhiteSpace(config.VirtualHost))
            {
                _connectionFactory.VirtualHost = config.VirtualHost;
            }
            if (!string.IsNullOrWhiteSpace(config.User))
            {
                _connectionFactory.UserName = config.User;
                _connectionFactory.Password = config.Password;
            }
            _logger = CurrentIocManager.CreateLogger<DefaultRabbitMqConnection>();
            Broker = config.Broker;
        }

        public string Name { get; }
        public string Broker { get; }
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection?.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public bool TryConnect()
        {
            _logger.LogInformation(
                $"RabbitMQ Client is trying to connect:{_connectionFactory.HostName}:{_connectionFactory.Port}/{_connectionFactory.VirtualHost}");

            lock (_syncRoot)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                    _disposed = false;
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }

                _logger.LogError("FATAL ERROR: RabbitMQ connections could not be created and opened");
                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
