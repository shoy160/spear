using Acb.Core.Logging;
using Newtonsoft.Json;
using Spear.Core.Message;
using Spear.Core.Transport;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Core.Runtime.Server.Impl
{
    public class DServiceExecutor : IServiceExecutor
    {
        private readonly ILogger _logger;

        public DServiceExecutor()
        {
            _logger = LogManager.Logger<DServiceExecutor>();
        }

        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            _logger.Info("接收到消息。");

            if (!message.IsInvoke)
                return;

            InvokeMessage remoteInvokeMessage;
            try
            {
                remoteInvokeMessage = message.GetContent<InvokeMessage>();
            }
            catch (Exception exception)
            {
                _logger.Error("将接收到的消息反序列化成 TransportMessage<RemoteInvokeMessage> 时发送了错误。", exception);
                return;
            }

            var entry = _serviceEntryLocate.Locate(remoteInvokeMessage);

            if (entry == null)
            {
                _logger.Error($"根据服务Id：{remoteInvokeMessage.ServiceId}，找不到服务条目。");
                return;
            }

            if (remoteInvokeMessage.Attachments != null)
            {
                foreach (var attachment in remoteInvokeMessage.Attachments)
                    RpcContext.GetContext().SetAttachment(attachment.Key, attachment.Value);
            }
            _logger.Debug("准备执行本地逻辑。");

            var resultMessage = new InvokeResultMessage();

            //是否需要等待执行。
            if (entry.Descriptor.WaitExecution())
            {
                //执行本地代码。
                await LocalExecuteAsync(entry, remoteInvokeMessage, resultMessage);
                //向客户端发送调用结果。
                await SendRemoteInvokeResult(sender, message.Id, resultMessage);
            }
            else
            {
                //通知客户端已接收到消息。
                await SendRemoteInvokeResult(sender, message.Id, resultMessage);
                //确保新起一个线程执行，不堵塞当前线程。
                await Task.Factory.StartNew(async () =>
                {
                    //执行本地代码。
                    await LocalExecuteAsync(entry, remoteInvokeMessage, resultMessage);
                }, TaskCreationOptions.LongRunning);
            }
        }
        private async Task LocalExecuteAsync(ServiceEntry entry, InvokeMessage remoteInvokeMessage, InvokeResultMessage resultMessage)
        {
            try
            {
                var cancelTokenSource = new CancellationTokenSource();
                if (!cancelTokenSource.IsCancellationRequested)
                {
                    var result = await entry.Func(remoteInvokeMessage.ServiceKey, remoteInvokeMessage.Parameters);
                    var task = result as Task;

                    if (task == null)
                    {
                        resultMessage.Data = result;
                    }
                    else
                    {
                        task.Wait();
                        var taskType = task.GetType().GetTypeInfo();
                        if (taskType.IsGenericType)
                            resultMessage.Data = taskType.GetProperty("Result").GetValue(task);
                    }

                    if (remoteInvokeMessage.DecodeJOject)
                    {
                        resultMessage.Data = JsonConvert.SerializeObject(resultMessage.Data);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error("执行本地逻辑时候发生了错误。", exception);
                resultMessage.Message = GetExceptionMessage(exception);
                resultMessage.Code = exception.HResult;
            }
        }

        private async Task SendRemoteInvokeResult(IMessageSender sender, string messageId, InvokeResultMessage resultMessage)
        {
            try
            {
                _logger.Debug("准备发送响应消息。");
                await sender.SendAndFlushAsync(TransportMessage.CreateInvokeResultMessage(messageId, resultMessage));
                _logger.Debug("响应消息发送成功。");
            }
            catch (Exception exception)
            {
                _logger.Error("发送响应消息时候发生了异常。", exception);
            }
        }

        private static string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var message = exception.Message;
            if (exception.InnerException != null)
            {
                message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
            }
            return message;
        }
    }
}
