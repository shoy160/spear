using Acb.Core.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.DotNetty.Adapter;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace Spear.DotNetty
{
    public class DotNettyClientFactory : IMicroClientFactory, IDisposable
    {
        private readonly Bootstrap _bootstrap;
        private readonly ILogger _logger;
        private readonly IMessageCoderFactory _coderFactory;
        private readonly IMicroExecutor _microExecutor;

        private readonly ConcurrentDictionary<EndPoint, Lazy<IMicroClient>> _clients;

        private static readonly AttributeKey<EndPoint> OrigEndPointKey =
            AttributeKey<EndPoint>.ValueOf(typeof(DotNettyClientFactory), nameof(EndPoint));

        private static readonly AttributeKey<IMessageSender> SenderKey =
            AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> ListenerKey =
            AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageListener));

        public DotNettyClientFactory(IMessageCoderFactory coderFactory, IMicroExecutor executor = null)
        {
            _coderFactory = coderFactory;
            _microExecutor = executor;
            _logger = LogManager.Logger<DotNettyClientFactory>();
            _bootstrap = GetBootstrap();
            _clients = new ConcurrentDictionary<EndPoint, Lazy<IMicroClient>>();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new MicroMessageHandler(_coderFactory.GetDecoder()));
                pipeline.AddLast(new DefaultChannelHandler(this));
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
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public IMicroClient CreateClient(EndPoint endPoint)
        {
            //_logger.Debug($"准备为服务端地址：{endPoint}创建客户端。");
            try
            {
                var lazyClient = _clients.GetOrAdd(endPoint, k => new Lazy<IMicroClient>(() =>
                    {
                        _logger.Debug($"创建客户端：{endPoint}创建客户端。");
                        var bootstrap = _bootstrap;
                        var channel = bootstrap.ConnectAsync(k).Result;
                        var listener = new MessageListener();
                        var sender = new DotNettyClientSender(_coderFactory.GetEncoder(), channel);
                        channel.GetAttribute(ListenerKey).Set(listener);
                        channel.GetAttribute(SenderKey).Set(sender);
                        channel.GetAttribute(OrigEndPointKey).Set(k);
                        return new MicroClient(sender, listener, _microExecutor);
                    }
                ));
                return lazyClient.Value;
            }
            catch (Exception ex)
            {
                _logger.Error("创建客户端失败", ex);
                _clients.TryRemove(endPoint, out _);
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

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly DotNettyClientFactory _factory;

            public DefaultChannelHandler(DotNettyClientFactory factory)
            {
                _factory = factory;
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                var k = context.Channel.GetAttribute(OrigEndPointKey).Get();
                _factory._logger.Debug($"删除客户端：{k}");
                _factory._clients.TryRemove(k, out _);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as MicroMessage;

                var messageListener = context.Channel.GetAttribute(ListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(SenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            #endregion Overrides of ChannelHandlerAdapter
        }
    }
}
