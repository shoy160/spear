using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Spear.Core.Message;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Protocol.Tcp.Adapter
{
    /// <summary> 服务端处理器 </summary>
    internal class ServerHandler : ChannelHandlerAdapter
    {
        private readonly ILogger _logger;
        private readonly Action<IChannelHandlerContext, MicroMessage> _readAction;

        public ServerHandler(ILogger logger, Action<IChannelHandlerContext, MicroMessage> readAction)
        {
            _readAction = readAction;
            _logger = logger;
        }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            _logger.LogDebug($"ConnectAsync,client:{remoteAddress},server:{localAddress}");
            return Task.CompletedTask;
        }

        public override Task DisconnectAsync(IChannelHandlerContext context)
        {
            _logger.LogDebug("DisconnectAsync");
            return Task.CompletedTask;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _logger.LogDebug($"ChannelActive:{context.Channel.RemoteAddress}");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogDebug($"ChannelInactive:{context.Channel.RemoteAddress}");
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            _logger.LogDebug($"ChannelRegistered:{context.Channel.RemoteAddress}");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            //_logger.Debug(message);
            if (!(message is MicroMessage msg))
                return;
            Task.Run(() => _readAction(context, msg));
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            //客户端主动断开需要应答，否则socket变成CLOSE_WAIT状态导致socket资源耗尽
            context.CloseAsync();
            _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
        }


    }
}
