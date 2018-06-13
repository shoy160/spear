using Acb.Core.Exceptions;
using Acb.Core.Logging;
using Spear.Core.Message;
using Spear.Core.Runtime.Server;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Spear.Core.Transport.Impl
{
    /// <summary> 默认的传输客户端 </summary>
    public class TransportClient : ITransportClient, IDisposable
    {
        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly ILogger _logger;

        /// <summary> 消息回调任务字典 </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>> _resultDictionary;

        public TransportClient(IMessageSender sender, IMessageListener listener, IServiceExecutor service)
        {
            _logger = LogManager.Logger<TransportClient>();
            _messageSender = sender;
            _messageListener = listener;
            _serviceExecutor = service;
            _messageListener.Received += MessageListener_Received;
            _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<TransportMessage>>();
        }

        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            //接收到消息
            _logger.Info($"接收到消息:{message.Id}");
            if (!_resultDictionary.TryGetValue(message.Id, out var task))
                return;

            if (message.IsResult)
            {
                var content = message.GetContent<InvokeResultMessage>();
                if (!string.IsNullOrEmpty(content.Message))
                {
                    task.TrySetException(new BusiException(content.Message, content.Code));
                }
                else
                {
                    task.SetResult(message);
                }
            }
            if (_serviceExecutor != null && message.IsInvoke)
                await _serviceExecutor.ExecuteAsync(sender, message);
        }

        private async Task<InvokeResultMessage> RegistResultCallbackAsync(string id)
        {
            //获取id的响应内容
            _logger.Debug($"准备获取Id为：{id}的响应内容。");
            var task = new TaskCompletionSource<TransportMessage>();
            _resultDictionary.TryAdd(id, task);
            try
            {
                var result = await task.Task;
                return result.GetContent<InvokeResultMessage>();
            }
            finally
            {
                //删除回调任务
                _resultDictionary.TryRemove(id, out _);
            }
        }

        public async Task<InvokeResultMessage> SendAsync(InvokeMessage message)
        {
            _logger.Debug("准备发送消息。");
            var transportMessage = message.Create();

            //注册结果回调
            var callback = RegistResultCallbackAsync(transportMessage.Id);
            try
            {
                //发送消息
                await _messageSender.SendAndFlushAsync(transportMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw new BusiException("与服务端通讯时发生了异常。");
            }

            return await callback;
        }

        public void Dispose()
        {
            _logger.Info("注销客户端");
            (_messageSender as IDisposable)?.Dispose();
            (_messageListener as IDisposable)?.Dispose();
            foreach (var taskCompletionSource in _resultDictionary.Values)
            {
                taskCompletionSource.TrySetCanceled();
            }
        }
    }
}
