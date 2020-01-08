using System.Security.Claims;
using System.Threading;

namespace Spear.Core.Session.Impl
{
    public class DefaultPrincipalAccessor : IPrincipalAccessor
    {
        public ClaimsPrincipal Principal
        {
            get
            {
                if (Thread.CurrentPrincipal is ClaimsPrincipal principal)
                    return principal;
                principal = new ClaimsPrincipal();
                Thread.CurrentPrincipal = principal;
                return principal;
            }
        }
    }
}
