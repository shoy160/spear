using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Message.Models;

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
            var ex = context.Exception;
            MessageResult messageResult;
            if (ex is SpearException busi)
            {
                messageResult = new MessageResult(busi.Message, busi.Code);
            }
            else
            {
                messageResult = new MessageResult(ex.Message);
            }
            context.Result = new JsonResult(messageResult)
            {
                StatusCode = messageResult.Code
            };
            context.HttpContext.Response.StatusCode = messageResult.Code;
            context.ExceptionHandled = true;
        }
    }
}
