using System.Threading.Tasks;
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
using Spear.Core.Message.Models;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Tcp.Adapter;
using Spear.Protocol.Tcp.Sender;

namespace Spear.Protocol.Tcp
{
    [Protocol(ServiceProtocol.Tcp)]
    public class DotNettyClientFactory : DMicroClientFactory
    {
        private readonly Bootstrap _bootstrap;

        private static readonly AttributeKey<ServiceAddress> ServiceAddressKey =
            AttributeKey<ServiceAddress>.ValueOf(typeof(DotNettyClientFactory), nameof(ServiceAddress));

        private static readonly AttributeKey<IMessageSender> SenderKey =
            AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageSender));
        private static readonly AttributeKey<IMessageListener> ListenerKey =
            AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyClientFactory), nameof(IMessageListener));

        public DotNettyClientFactory(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory, IMicroExecutor executor = null)
            : base(loggerFactory, codecFactory, executor)
        {
            _bootstrap = GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                pipeline.AddLast(new MicroMessageHandler<MessageResult>(CodecFactory.GetDecoder()));
                pipeline.AddLast(new ClientHandler((context, message) =>
                {
                    var messageListener = context.Channel.GetAttribute(ListenerKey).Get();
                    var messageSender = context.Channel.GetAttribute(SenderKey).Get();
                    messageListener.OnReceived(messageSender, message);
                }, channel =>
                {
                    var k = channel.GetAttribute(ServiceAddressKey).Get();
                    Remove(k);
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
        protected override async Task<IMicroClient> Create(ServiceAddress serviceAddress)
        {
            var bootstrap = _bootstrap;
            var channel = await bootstrap.ConnectAsync(serviceAddress.ToEndPoint(false));
            var listener = new MessageListener();
            var sender = new DotNettyClientSender(CodecFactory.GetEncoder(), channel);
            channel.GetAttribute(ListenerKey).Set(listener);
            channel.GetAttribute(SenderKey).Set(sender);
            channel.GetAttribute(ServiceAddressKey).Set(serviceAddress);
            return new MicroClient(sender, listener, MicroExecutor, LoggerFactory);
        }
    }
}
