using Acb.Core.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Spear.Core.Message;
using Spear.Core.Micro;
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

        private readonly ConcurrentDictionary<EndPoint, Lazy<IMicroClient>> _clients =
            new ConcurrentDictionary<EndPoint, Lazy<IMicroClient>>();

        private static readonly AttributeKey<EndPoint> OrigEndPointKey =
            AttributeKey<EndPoint>.ValueOf(typeof(DotNettyClientFactory), nameof(EndPoint));

        private static readonly AttributeKey<IMicroSender> SenderKey = AttributeKey<IMicroSender>.ValueOf(typeof(DotNettyClientFactory), nameof(IMicroSender));
        private static readonly AttributeKey<IMicroListener> ListenerKey = AttributeKey<IMicroListener>.ValueOf(typeof(DotNettyClientFactory), nameof(IMicroListener));

        public DotNettyClientFactory(IMessageCoderFactory coderFactory, IMicroExecutor executor = null)
        {
            _coderFactory = coderFactory;
            _microExecutor = executor;
            _logger = LogManager.Logger<DotNettyClientFactory>();
            _bootstrap = GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new MicroMessageHandler(_coderFactory.GetDecoder()));
                pipeline.AddLast(new ClientHandler(channel =>
                {
                    var k = channel.GetAttribute(OrigEndPointKey).Get();
                    _logger.Debug($"删除客户端：{k}");
                    _clients.TryRemove(k, out _);
                }, async (context, msg) =>
                {
                    var listener = context.Channel.GetAttribute(ListenerKey).Get();
                    var sender = context.Channel.GetAttribute(SenderKey).Get();
                    await listener.OnReceived(sender, msg);
                }));
            }));
        }

        private static Bootstrap GetBootstrap()
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(new MultithreadEventLoopGroup(1));

            return bootstrap;
        }

        /// <summary> 创建客户端 </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public IMicroClient CreateClient(EndPoint endPoint)
        {
            var key = endPoint;
            try
            {
                return _clients.GetOrAdd(key, k => new Lazy<IMicroClient>(() =>
                    {
                        _logger.Debug($"准备为服务端地址：{key}创建客户端。");
                        var bootstrap = _bootstrap;
                        var channel = bootstrap.ConnectAsync(k).Result;
                        var listener = new MicroListener();
                        var sender = new DotNettyClientSender(_coderFactory.GetEncoder(), channel);
                        channel.GetAttribute(OrigEndPointKey).Set(k);
                        channel.GetAttribute(ListenerKey).Set(listener);
                        channel.GetAttribute(SenderKey).Set(sender);
                        return new MicroClient(sender, listener, _microExecutor);
                    }
                )).Value;
            }
            catch (Exception ex)
            {
                _logger.Error("创建客户端失败", ex);
                _clients.TryRemove(key, out _);
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
