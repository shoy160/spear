using Acb.Core.Exceptions;
using Acb.Core.Logging;
using Spear.Core.Message;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Spear.Core.Micro
{
    /// <summary> 默认服务客户端 </summary>
    public class MicroClient : IMicroClient, IDisposable
    {
        private readonly IMicroSender _sender;
        private readonly IMicroListener _listener;
        private readonly IMicroExecutor _executor;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<MicroMessage>> _resultDictionary =
            new ConcurrentDictionary<string, TaskCompletionSource<MicroMessage>>();

        public MicroClient(IMicroSender sender, IMicroListener listener, IMicroExecutor executor)
        {
            _sender = sender;
            _listener = listener;
            _executor = executor;
            _logger = LogManager.Logger<MicroClient>();
            listener.Received += ListenerOnReceived;
        }

        private async Task ListenerOnReceived(IMicroSender sender, MicroMessage message)
        {
            if (!_resultDictionary.TryGetValue(message.Id, out var task))
                return;

            if (message.IsResult)
            {
                var content = message.GetContent<ResultMessage>();
                if (content.Code != 200)
                {
                    task.TrySetException(new BusiException(content.Message, content.Code));
                }
                else
                {
                    task.SetResult(message);
                }
            }
            if (_executor != null && message.IsInvoke)
                await _executor.Execute(sender, message);
        }

        private async Task<T> RegistCallbackAsync<T>(string messageId)
        {
            var task = new TaskCompletionSource<MicroMessage>();
            _resultDictionary.TryAdd(messageId, task);
            try
            {
                var result = await task.Task;
                return result.GetContent<T>();
            }
            finally
            {
                //删除回调任务
                _resultDictionary.TryRemove(messageId, out _);
            }
        }

        public async Task<T> Send<T>(object message)
        {
            try
            {
                _logger.Debug("准备发送消息");
                var microMessage = new MicroMessage(message);
                var callback = RegistCallbackAsync<T>(microMessage.Id);
                try
                {
                    //发送
                    await _sender.Send(microMessage);
                }
                catch (Exception exception)
                {
                    _logger.Error("与服务端通讯时发生了异常", exception);
                    throw new BusiException("与服务端通讯时发生了异常");
                }
                _logger.Debug("消息发送成功");
                return await callback;
            }
            catch (Exception ex)
            {
                _logger.Error("消息发送失败。", ex);
                throw new BusiException("消息发送失败");
            }
        }

        public async Task<ResultMessage> Send(InvokeMessage message)
        {
            return await Send<ResultMessage>(message);
        }

        public void Dispose()
        {
            (_sender as IDisposable)?.Dispose();
            (_listener as IDisposable)?.Dispose();
            foreach (var taskCompletionSource in _resultDictionary.Values)
            {
                taskCompletionSource.TrySetCanceled();
            }
        }
    }
}
