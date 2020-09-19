using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Dependency;
using Spear.Core.Exceptions;
using System.Linq;

namespace Spear.WebApi.Filters
{
    /// <summary> 模型验证过滤器 </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly bool _validation;
        private readonly ILogger _logger;

        /// <summary> 构造函数 </summary>
        /// <param name="validation"></param>
        public ValidateModelAttribute(bool validation = true)
        {
            _validation = validation;
            _logger = CurrentIocManager.CreateLogger<ValidateModelAttribute>();
        }

        /// <summary> 请求执行时验证模型 </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!_validation || context.ModelState.IsValid)
                return;
            var errordict = context.ModelState.FirstOrDefault(t => t.Value.Errors.Count > 0);
            if (errordict.Key == null)
                return;
            var value = errordict.Value.Errors[0].ErrorMessage;
            if (string.IsNullOrWhiteSpace(value))
            {
                var ex = errordict.Value.Errors[0].Exception;
                if (ex is BusiException exception)
                {
                    value = exception.Message;
                }
                else
                {
                    _logger.LogError(ex, ex.Message);
                    value = $"参数{errordict.Key}验证失败";
                }
            }
            context.Result = new JsonResult(DResult.Error(value, ErrorCodes.ParamaterError));
            base.OnActionExecuting(context);
        }
    }
}
