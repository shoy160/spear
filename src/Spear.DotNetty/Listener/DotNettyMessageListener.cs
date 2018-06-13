using Acb.Core.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Spear.Core.Message;
using Spear.Core.Transport;
using Spear.Core.Transport.Impl;
using Spear.DotNetty.Adapter;
using Spear.DotNetty.Sender;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.DotNetty.Listener
{
    public class DotNettyMessageListener : MessageListener, IDisposable
    {
        private readonly IMessageDecoder _messageDecoder;
        private readonly IMessageEncoder _messageEncoder;
        private IChannel _channel;
        private readonly ILogger _logger;

        public DotNettyMessageListener(IMessageCoderFactory coderFactory)
        {
            _messageEncoder = coderFactory.GetEncoder();
            _messageDecoder = coderFactory.GetDecoder();
            _logger = LogManager.Logger<DotNettyMessageListener>();
        }
        public async Task StartAsync(EndPoint endPoint)
        {
            _logger.Debug($"准备启动服务主机，监听地址：{endPoint}。");

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new TransportMessageChannelHandlerAdapter(_messageDecoder));
                    pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                    {
                        var sender = new DotNettyServerMessageSender(_messageEncoder, contenxt);
                        await OnReceived(sender, message);
                    }, _logger));
                }));
            _channel = await bootstrap.BindAsync(endPoint);
            _logger.Debug($"服务主机启动成功，监听地址：{endPoint}。");
        }

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;
            private readonly ILogger _logger;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger)
            {
                _readAction = readAction;
                _logger = logger;
            }


            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                Task.Run(() =>
                {
                    var transportMessage = (TransportMessage)message;
                    _readAction(context, transportMessage);
                });
            }

            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.CloseAsync();//客户端主动断开需要应答，否则socket变成CLOSE_WAIT状态导致socket资源耗尽
                _logger.Error($"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。", exception);
            }
        }
    }
}
