using Acb.Core.Extensions;
using Acb.Core.Tests;
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
    public class Client : BaseTest
    {
        private readonly int _port;
        private IChannel _channel;

        public Client(int port)
        {
            _port = port;
            Start().Wait();
        }

        private async Task Start()
        {
            var bootstrap = new Bootstrap();
            var group = new MultithreadEventLoopGroup();
            bootstrap.Channel<TcpServerSocketChannel>();
            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Group(group)
                .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                {
                    var pipeline = c.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new ClientHandler());
                }));
            _channel = await bootstrap.ConnectAsync(IPAddress.Parse("127.0.0.1"), _port);
        }

        public override Task OnCommand(string cmd)
        {
            var msgArgs = cmd.Split(new[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries);
            int repeat = 1, thread = 1;
            if (msgArgs.Length > 1)
                repeat = msgArgs[1].CastTo(1);
            if (msgArgs.Length > 2)
                thread = msgArgs[2].CastTo(1);
            var message = msgArgs[0];

            Task.Run(async () =>
            {
                var result = await CodeTimer.Time("netty", repeat, async () =>
                {
                    var data = message.Encode();
                    await _channel.WriteAndFlushAsync(data);
                }, thread);
                Console.WriteLine(result.ToString());
            });

            return base.OnCommand(cmd);
        }

        public override void Dispose()
        {
            _channel?.DisconnectAsync();
            base.Dispose();
        }
    }

    public class ClientHandler : ChannelHandlerAdapter
    {
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
            base.ChannelReadComplete(context);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = message.Decode();
            Console.WriteLine($"receive:{msg}");
            base.ChannelRead(context, message);
        }
    }
}
