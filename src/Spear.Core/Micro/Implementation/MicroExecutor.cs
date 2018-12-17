using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Acb.Core.Dependency;
using Acb.Core.Extensions;
using Acb.Core.Logging;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;

namespace Spear.Core.Micro.Implementation
{
    /// <inheritdoc />
    /// <summary> 默认服务执行者 </summary>
    public class MicroExecutor : IMicroExecutor
    {
        private readonly ILogger _logger;
        private readonly IMicroEntryFactory _entryFactory;
        public MicroExecutor(IMicroEntryFactory entryFactory)
        {
            _entryFactory = entryFactory;
            _logger = LogManager.Logger<MicroExecutor>();
        }

        private async Task LocalExecute(InvokeMessage invokeMessage, ResultMessage result)
        {
            try
            {
                var service = _entryFactory.Find(invokeMessage.ServiceId);
                var args = new List<object>();
                var parameters = invokeMessage.Parameters ?? new Dictionary<string, object>();
                foreach (var parameter in service.GetParameters())
                {
                    if (parameters.ContainsKey(parameter.Name))
                    {
                        var parameterType = parameter.ParameterType;
                        args.Add(parameters[parameter.Name].CastTo(parameterType));
                    }
                    else
                    {
                        args.Add(parameter.DefaultValue);
                    }
                }

                var instance = CurrentIocManager.Resolve(service.DeclaringType);
                var data = service.Invoke(instance, args.ToArray());
                if (!(data is Task task))
                {
                    result.Data = data;
                }
                else
                {
                    await task;
                    var taskType = task.GetType().GetTypeInfo();
                    if (taskType.IsGenericType)
                    {
                        var prop = taskType.GetProperty("Result");
                        if (prop != null)
                            result.Data = prop.GetValue(task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("执行本地逻辑时候发生了错误。", ex);
                result.Message = ex.Message;
                result.Code = 500;
            }
        }

        private async Task SendResult(IMessageSender sender, string messageId, ResultMessage result)
        {
            try
            {
                await sender.Send(result.Create(messageId));
            }
            catch (Exception ex)
            {
                _logger.Error("发送响应消息异常", ex);
            }
        }

        public async Task Execute(IMessageSender sender, MicroMessage message)
        {
            if (!message.IsInvoke)
                return;
            var invokeMessage = message.GetContent<InvokeMessage>();
            var result = new ResultMessage();
            if (invokeMessage.IsNotice)
            {
                //向客户端发送结果
                await SendResult(sender, message.Id, result);

                //确保新起一个线程执行，不堵塞当前线程
                await Task.Factory.StartNew(async () =>
                {
                    //执行本地代码
                    await LocalExecute(invokeMessage, result);
                }, TaskCreationOptions.LongRunning);
                return;
            }

            //执行本地代码
            await LocalExecute(invokeMessage, result);
            //向客户端发送结果
            await SendResult(sender, message.Id, result);
        }
    }
}
