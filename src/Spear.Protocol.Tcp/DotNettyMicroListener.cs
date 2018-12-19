using Acb.Core.Logging;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Spear.Core.Message;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Tcp.Adapter;
using Spear.Protocol.Tcp.Sender;
using System;
using System.Threading.Tasks;

namespace Spear.Protocol.Tcp
{
    public class DotNettyMicroListener : MicroListener, IDisposable
    {
        private IChannel _channel;
        private readonly ILogger _logger;
        private readonly IMessageCoderFactory _coderFactory;

        public DotNettyMicroListener(IMessageCoderFactory coderFactory)
        {
            _logger = LogManager.Logger<DotNettyMicroListener>();
            _coderFactory = coderFactory;
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            _logger.Debug($"准备启动服务主机，监听地址：{serviceAddress}。");
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
                    pipeline.AddLast(new MicroMessageHandler(_coderFactory.GetDecoder()));
                    pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                    {
                        var sender = new DotNettyServerSender(_coderFactory.GetEncoder(), contenxt);
                        await OnReceived(sender, message);
                    }));
                }));
            var endPoint = serviceAddress.ToEndPoint();
            _channel = await bootstrap.BindAsync(endPoint);
            _logger.Info($"服务主机启动成功，监听地址：{serviceAddress}。");
        }

        public override Task Stop()
        {
            return Task.Run(() =>
            {
                Dispose();
                //await _channel.EventLoop.ShutdownGracefullyAsync();
                //await _channel.CloseAsync();
            });
        }

        public void Dispose()
        {
            if (_channel == null)
                return;
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }
    }
}
