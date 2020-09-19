using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spear.Core;
using Spear.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.WebApi.Filters
{
    /// <inheritdoc />
    /// <summary> 默认的异常处理 </summary>
    public class DExceptionFilter : IAsyncExceptionFilter
    {
        /// <summary> 业务消息过滤 </summary>
        public static Action<DResult> ResultFilter;
        /// <inheritdoc />
        /// <summary> 异常处理 </summary>
        /// <param name="context"></param>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var json = await ExceptionHandler.HandlerAsync(context.Exception);
            if (json == null)
                return;
            ResultFilter?.Invoke(json);
            const int code = (int)HttpStatusCode.OK;
            //var code = json.Code;
            context.Result = new JsonResult(json)
            {
                StatusCode = code
            };
            context.HttpContext.Response.StatusCode = code;
            context.ExceptionHandled = true;
        }
    }
}
