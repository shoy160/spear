using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Dependency;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Spear.Core.Serialize;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Spear.Framework
{
    /// <summary> 异常处理类 </summary>
    public static class ExceptionHandler
    {
        private static ILogger Logger => CurrentIocManager.CreateLogger(typeof(ExceptionHandler));

        /// <summary> 异常事件 </summary>
        public static event Action<Exception> OnException;

        /// <summary> 业务异常事件 </summary>
        public static event Action<BusiException> OnBusiException;

        private class LogErrorMsg
        {
            public string Message { get; set; }
            public string Url { get; set; }
            public string Form { get; set; }
            public string Token { get; set; }
            public string ClientIp { get; set; }
            public string UserAgent { get; set; }
        }

        /// <summary> 异常处理 </summary>
        /// <param name="exception"></param>
        /// <param name="requestBody"></param>
        public static DResult Handler(Exception exception, string requestBody = null)
        {
            return HandlerAsync(exception, requestBody).SyncRun();
        }

        /// <summary> 异常处理 </summary>
        /// <param name="exception"></param>
        /// <param name="requestBody"></param>
        public static async Task<DResult> HandlerAsync(Exception exception, string requestBody = null)
        {
            DResult result = null;
            var ex = exception.GetBaseException();
            switch (ex)
            {
                case BusiException busi:
                    OnBusiException?.Invoke(busi);
                    result = DResult.Error(busi.Message, busi.Code);
                    break;
                case OperationCanceledException _:
                case SocketException se when se.Message == "Operation canceled":
                    //操作取消
                    break;
                default:
                    OnException?.Invoke(ex);
                    var msg = new LogErrorMsg
                    {
                        Message = ex.Message
                    };
                    if (CurrentIocManager.Context != null)
                    {
                        var context = CurrentIocManager.Context;
                        var wrap = CurrentIocManager.ContextWrap;
                        msg.Url = context.RawUrl();
                        msg.ClientIp = wrap.ClientIp;
                        msg.UserAgent = wrap.UserAgent;
                        msg.Form = string.IsNullOrWhiteSpace(requestBody)
                            ? await wrap.FromBody<string>()
                            : requestBody;
                        msg.Token = wrap.Authorization ?? string.Empty;
                    }
                    Logger.LogError(ex, JsonHelper.ToJson(msg));
                    result = ErrorCodes.SystemError.CodeResult();
                    break;
            }

            return result;
        }
    }
}
