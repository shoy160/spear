using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Tcp.Adapter;
using Spear.Protocol.Tcp.Sender;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Spear.Protocol.Tcp
{
    [Protocol(ServiceProtocol.Tcp)]
    public class DotNettyClientFactory : IMicroClientFactory, IDisposable
    {
        private readonly Bootstrap _bootstrap;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DotNettyClientFactory> _logger;
        private readonly IMessageCodecFactory _codecFactory;
        private readonly IMicroExecutor _microExecutor;

        private readonly ConcurrentDictionary<string, Lazy<Task<IMicroClient>>> _clients;

        private static readonly AttributeKey<ServiceAddress> ServiceAddressKey =
            AttributeKey<ServiceAddress>.ValueOf(typeof(DotNettyClientFactory), nameof(ServiceAddress));

        private static readonly AttributeKey<IMessageSender> SenderKey =
            AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> ListenerKey =
            AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageListener));

        public DotNettyClientFactory(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory, IMicroExecutor executor = null)
        {
            _codecFactory = codecFactory;
            _microExecutor = executor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<DotNettyClientFactory>();
            _bootstrap = GetBootstrap();
            _clients = new ConcurrentDictionary<string, Lazy<Task<IMicroClient>>>();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new MicroMessageHandler(_codecFactory.GetDecoder()));
                pipeline.AddLast(new ClientHandler((context, message) =>
                {
                    var messageListener = context.Channel.GetAttribute(ListenerKey).Get();
                    var messageSender = context.Channel.GetAttribute(SenderKey).Get();
                    messageListener.OnReceived(messageSender, message);
                }, channel =>
                {
                    var k = channel.GetAttribute(ServiceAddressKey).Get();
                    _logger.LogDebug($"删除客户端：{k}");
                    _clients.TryRemove(k.ToString(), out _);
                }, loggerFactory));
            }));
        }

        private static Bootstrap GetBootstrap()
        {
            IEventLoopGroup group;
            var bootstrap = new Bootstrap();
            //if (AppConfig.ServerOptions.Libuv)
            //{
            //    group = new EventLoopGroup();
            //    bootstrap.Channel<TcpServerChannel>();
            //}
            //else
            {
                group = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group);

            return bootstrap;
        }

        /// <inheritdoc />
        /// <summary> 创建客户端 </summary>
        /// <param name="serviceAddress">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public Task<IMicroClient> CreateClient(ServiceAddress serviceAddress)
        {
            //_logger.Debug($"准备为服务端地址：{endPoint}创建客户端。");
            try
            {
                var lazyClient = _clients.GetOrAdd(serviceAddress.ToString(), k => new Lazy<Task<IMicroClient>>(async () =>
                  {
                      _logger.LogDebug($"创建客户端：{serviceAddress}创建客户端。");
                      var bootstrap = _bootstrap;
                      var channel = await bootstrap.ConnectAsync(serviceAddress.ToEndPoint(false));
                      var listener = new MessageListener();
                      var sender = new DotNettyClientSender(_codecFactory.GetEncoder(), channel);
                      channel.GetAttribute(ListenerKey).Set(listener);
                      channel.GetAttribute(SenderKey).Set(sender);
                      channel.GetAttribute(ServiceAddressKey).Set(serviceAddress);
                      return new MicroClient(sender, listener, _microExecutor, _loggerFactory);
                  }
                ));
                return lazyClient.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建客户端失败");
                _clients.TryRemove(serviceAddress.ToString(), out _);
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values.Where(i => i.IsValueCreated))
            {
                (client.Value as IDisposable)?.Dispose();
            }
        }
    }
}
