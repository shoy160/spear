using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Spear.Core.Exceptions;
using Spear.Core.Helper;
using Spear.Core.Timing;
using Spear.Core.Extensions;
using System;

namespace Spear.WebApi.Filters
{
    /// <inheritdoc />
    /// <summary> 内部基础验证 </summary>
    public class AppTicketAttribute : ActionFilterAttribute
    {
        private const string HeaderKey = "App-Ticket";
        private const string CommonTicketConfig = "commonTicket";
        private readonly bool _isValide; // 设置是否需要启动接口安全检查

        public AppTicketAttribute(bool isValide = true)
        {
            _isValide = isValide;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (!ValidTicket(actionContext.HttpContext.Request))
            {
                throw ErrorCodes.ClientError.CodeException<ErrorCodes>();
            }
            base.OnActionExecuting(actionContext);
        }

        private bool ValidTicket(HttpRequest request)
        {
            try
            {
                if (!_isValide)
                    return true;
                if (!request.Headers.ContainsKey(HeaderKey))
                    return false;
                request.Headers.TryGetValue(HeaderKey, out var ticket);
                if (string.IsNullOrWhiteSpace(ticket))
                    return false;
                var commonTicket = CommonTicketConfig.Config<string>();
                if (!string.IsNullOrWhiteSpace(commonTicket) &&
                    string.Equals(commonTicket, ticket, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                var timestamp = ticket.ToString().Substring(0, 10).CastTo(0L);
                if (timestamp.FromTimestamp().AddMinutes(5) < Clock.Now)
                    throw ErrorCodes.ClientTimeoutError.CodeException<ErrorCodes>();
                // 规则 App-Ticket=时间戳秒 + Md532(key + 时间戳秒).ToLower()
                var ticketKey = "ticket".Config<string>();
                var value = timestamp + EncryptHelper.Hash($"{ticketKey}{timestamp}", EncryptHelper.HashFormat.MD532).ToLower();
                return value == ticket;
            }
            catch
            {
                return false;
            }
        }
    }
}
