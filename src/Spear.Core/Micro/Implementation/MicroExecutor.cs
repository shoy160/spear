using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Message;
using Spear.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    /// <inheritdoc />
    /// <summary> 默认服务执行者 </summary>
    public class MicroExecutor : IMicroExecutor
    {
        private readonly ILogger<MicroExecutor> _logger;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly IServiceProvider _provider;
        public MicroExecutor(ILogger<MicroExecutor> logger, IServiceProvider provider, IMicroEntryFactory entryFactory)
        {
            _entryFactory = entryFactory;
            _logger = logger;
            _provider = provider;
        }

        private async Task LocalExecute(InvokeMessage invokeMessage, ResultMessage result)
        {
            try
            {
                _logger.LogDebug(JsonConvert.SerializeObject(invokeMessage));
                var service = _entryFactory.Find(invokeMessage.ServiceId);
                var args = new List<object>();
                var parameters = invokeMessage.Parameters ?? new Dictionary<string, object>();
                foreach (var parameter in service.GetParameters())
                {
                    if (parameters.ContainsKey(parameter.Name))
                    {
                        var parameterType = parameter.ParameterType;
                        var arg = parameters[parameter.Name].CastTo(parameterType);
                        args.Add(arg);
                    }
                    else if (parameter.HasDefaultValue)
                    {
                        args.Add(parameter.DefaultValue);
                    }
                }

                var instance = _provider.GetService(service.DeclaringType);

                var fastInvoke = FastInvoke.GetMethodInvoker(service);
                var data = fastInvoke(instance, args.ToArray());
                //var data = service.Invoke(instance, args.ToArray());
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
                _logger.LogError(ex, "执行本地逻辑时候发生了错误。");
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
                _logger.LogError(ex, "发送响应消息异常");
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
