using Acb.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Spear.Protocol.Http.Filters
{
    /// <inheritdoc />
    /// <summary> 默认的异常处理 </summary>
    public class DExceptionFilter : IExceptionFilter
    {
        /// <inheritdoc />
        /// <summary> 异常处理 </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            var json = ExceptionHandler.Handler(context.Exception);
            if (json == null)
                return;
            const int code = (int)HttpStatusCode.InternalServerError;
            context.Result = new JsonResult(json)
            {
                StatusCode = code
            };
            context.HttpContext.Response.StatusCode = code;
            context.ExceptionHandled = true;
        }
    }
}
