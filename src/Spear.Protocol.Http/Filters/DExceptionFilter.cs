using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spear.Core;
using Spear.Core.Message;

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
            ResultMessage result;
            if (ex is SpearException busi)
            {
                result = new ResultMessage(busi.Message, busi.Code);
            }
            else
            {
                result = new ResultMessage(ex.Message);
            }
            context.Result = new JsonResult(result)
            {
                StatusCode = result.Code
            };
            context.HttpContext.Response.StatusCode = result.Code;
            context.ExceptionHandled = true;
        }
    }
}
