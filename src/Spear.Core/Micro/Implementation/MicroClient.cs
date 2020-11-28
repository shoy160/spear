using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;

namespace Spear.Core.Micro.Implementation
{
    /// <summary> 默认服务客户端 </summary>
    public class MicroClient : IMicroClient, IDisposable
    {
        private readonly IMessageSender _sender;
        private readonly IMessageListener _listener;
        private readonly IMicroExecutor _executor;
        private readonly ILogger<MicroClient> _logger;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<MessageResult>> _resultDictionary;

        public MicroClient(IMessageSender sender, IMessageListener listener, IMicroExecutor executor, ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _listener = listener;
            _executor = executor;
            _logger = loggerFactory.CreateLogger<MicroClient>();
            _resultDictionary = new ConcurrentDictionary<string, TaskCompletionSource<MessageResult>>();
            listener.Received += ListenerOnReceived;
        }

        private async Task ListenerOnReceived(IMessageSender sender, DMessage message)
        {
            if (!_resultDictionary.TryGetValue(message.Id, out var task))
                return;

            if (message is MessageResult result)
            {
                if (result.Code != 200)
                {
                    task.TrySetException(new SpearException(result.Message, result.Code));
                }
                else
                {
                    task.SetResult(result);
                }
            }
            if (_executor != null && message is InvokeMessage invokeMessage)
                await _executor.Execute(sender, invokeMessage);
        }

        private async Task<MessageResult> RegistCallbackAsync(string messageId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备获取Id为：{messageId}的响应内容。");
            var task = new TaskCompletionSource<MessageResult>();
            _resultDictionary.TryAdd(messageId, task);
            try
            {
                var result = await task.Task;
                return result;
            }
            finally
            {
                //删除回调任务
                _resultDictionary.TryRemove(messageId, out _);
            }
        }

        public async Task<MessageResult> Send(InvokeMessage message)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息");
                var callback = RegistCallbackAsync(message.Id);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"{_sender.GetType()}:send :{JsonConvert.SerializeObject(message)}");
                //发送
                await _sender.Send(message);
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("消息发送成功");
                return await callback;
            }
            finally
            {
                watch.Stop();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"send message {watch.ElapsedMilliseconds} ms");
            }
        }

        //public async Task<ResultMessage> Send(InvokeMessage message)
        //{
        //    return await Send<ResultMessage>(message);
        //}

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
