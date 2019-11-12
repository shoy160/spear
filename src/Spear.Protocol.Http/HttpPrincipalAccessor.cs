using Microsoft.AspNetCore.Http;
using Spear.Core.Session;
using System.Security.Claims;
using System.Threading;

namespace Spear.Protocol.Http
{
    public class HttpPrincipalAccessor : IPrincipalAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpPrincipalAccessor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }


        public ClaimsPrincipal Principal
        {
            get
            {
                if (_contextAccessor.HttpContext?.User != null)
                    return _contextAccessor.HttpContext.User;
                if (Thread.CurrentPrincipal is ClaimsPrincipal principal)
                    return principal;
                principal = new ClaimsPrincipal();
                Thread.CurrentPrincipal = principal;
                return principal;
            }
        }

    }
}
