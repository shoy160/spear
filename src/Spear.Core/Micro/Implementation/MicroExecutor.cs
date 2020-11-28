using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Session;

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

        private async Task LocalExecute(MicroEntry entry, InvokeMessage invokeMessage, MessageResult messageResult)
        {
            try
            {
                //if (_logger.IsEnabled(LogLevel.Debug))
                //    _logger.LogDebug(JsonConvert.SerializeObject(invokeMessage));
                if (entry.IsNotify)
                {
                    await entry.Invoke(invokeMessage.Parameters);
                }
                else
                {
                    var data = await entry.Invoke(invokeMessage.Parameters);
                    if (!(data is Task task))
                    {
                        messageResult.Content = data;// new DynamicMessage(data);
                    }
                    else
                    {
                        await task;
                        var taskType = task.GetType().GetTypeInfo();
                        if (taskType.IsGenericType)
                        {
                            var prop = taskType.GetProperty("Result");
                            if (prop != null)
                                messageResult.Content = prop.GetValue(task); // new DynamicMessage(prop.GetValue(task));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogError(ex, "执行本地逻辑时候发生了错误。");
                messageResult.Message = ex.Message;
                messageResult.Code = 500;
            }
        }

        private async Task SendResult(IMessageSender sender, string messageId, DMessage result)
        {
            try
            {
                result.Id = messageId;
                await sender.Send(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送响应消息异常");
            }
        }

        /// <summary> 执行 </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Execute(IMessageSender sender, InvokeMessage message)
        {
            var accessor = _provider.GetService<IPrincipalAccessor>();
            if (accessor != null && message.Headers != null && message.Headers.Any())
            {
                var session = new SessionDto();
                //解析Claims
                if (message.Headers.TryGetValue(SpearClaimTypes.HeaderUserId, out var userId))
                    session.UserId = userId;
                if (message.Headers.TryGetValue(SpearClaimTypes.HeaderTenantId, out var tenantId))
                    session.TenantId = tenantId;
                //username
                if (message.Headers.TryGetValue(SpearClaimTypes.HeaderUserName, out var userName))
                    session.UserName = HttpUtility.UrlDecode(userName);
                //role
                if (message.Headers.TryGetValue(SpearClaimTypes.HeaderRole, out var role))
                    session.Role = HttpUtility.UrlDecode(role);
                accessor.SetSession(session);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(JsonConvert.SerializeObject(message));
            var entry = _entryFactory.Find(message.ServiceId);
            if (entry == null)
            {
                //向客户端发送结果
                await SendResult(sender, message.Id, new MessageResult("服务未找到"));
                return;
            }
            var result = new MessageResult();
            if (entry.IsNotify)
            {
                //向客户端发送结果
                await SendResult(sender, message.Id, result);

                //确保新起一个线程执行，不堵塞当前线程
                await Task.Factory.StartNew(async () =>
                {
                    //执行本地代码
                    await LocalExecute(entry, message, result);
                }, TaskCreationOptions.LongRunning);
                return;
            }

            //执行本地代码
            await LocalExecute(entry, message, result);
            //向客户端发送结果
            await SendResult(sender, message.Id, result);
        }
    }
}
