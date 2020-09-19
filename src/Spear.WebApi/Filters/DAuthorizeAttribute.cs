using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Spear.Core.Exceptions;
using Spear.Core.Timing;
using System.Linq;

namespace Spear.WebApi.Filters
{
    /// <inheritdoc />
    /// <summary> 默认身份认证过滤器 </summary>
    public class DAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _scheme;

        public DAuthorizeAttribute(string scheme = "spear")
        {
            _scheme = scheme;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ticket = context.HttpContext.Request.VerifyTicket<ClientTicket>(_scheme);

            if (context.ActionDescriptor.EndpointMetadata.Any(t => t is AllowAnonymousAttribute))
            {
                base.OnActionExecuting(context);
                return;
            }

            BaseValidate(ticket);

            var controller = context.Controller as DController;

            controller?.AuthorizeValidate(context.HttpContext);
            base.OnActionExecuting(context);
        }

        /// <summary> 验证令牌 </summary>
        /// <param name="ticket"></param>
        protected virtual void BaseValidate(IClientTicket ticket)
        {
            if (ticket == null)
                throw ErrorCodes.NeedTicket.CodeException();
            if (ticket.ExpiredTime.HasValue && ticket.ExpiredTime.Value < Clock.Now)
                throw ErrorCodes.InvalidTicket.CodeException();
        }
    }
}
