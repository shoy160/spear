using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Tests.Netty
{
    public class Server : BaseTest
    {
        private readonly int _port;
        private IChannel _channel;

        public Server(int port)
        {
            _port = port;
            Start().Wait();
        }

        private async Task Start()
        {
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 8192)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(bossGroup, workerGroup)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new ServerHandler());
                }));
            _channel = await bootstrap.BindAsync(IPAddress.Any, _port);
            Console.WriteLine($"listen at {_channel.LocalAddress}");
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            base.Dispose();
        }
    }

    public class ServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine($"client {context.Channel.RemoteAddress} connected");
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine($"client {context.Channel.RemoteAddress} inactive");
            base.ChannelInactive(context);
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            Console.WriteLine($"client {context.Channel.RemoteAddress} disconnected");
            return base.DisconnectAsync(context);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
            base.ChannelReadComplete(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message.Decode();
            Console.WriteLine($"{context.Channel.RemoteAddress}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]:\t{msg},");
            base.ChannelRead(context, message);
        }
    }
}
