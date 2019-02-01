using DotNetty.Transport.Channels;
using Spear.Core.Message;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Spear.Protocol.Tcp.Adapter
{
    /// <summary> 客户端处理器 </summary>
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly Action<IChannel> _removeAction;
        private readonly Action<IChannelHandlerContext, MicroMessage> _readAction;
        private readonly ILogger _logger;

        public ClientHandler(ILogger logger, Action<IChannel> removeAction, Action<IChannelHandlerContext, MicroMessage> readAction)
        {
            _removeAction = removeAction;
            _readAction = readAction;
            _logger = logger;
        }
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _removeAction?.Invoke(context.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            _logger.LogDebug(JsonConvert.SerializeObject(message));
            if (!(message is MicroMessage msg))
                return;
            _readAction?.Invoke(context, msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogError(exception, $"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
        }
    }
}
