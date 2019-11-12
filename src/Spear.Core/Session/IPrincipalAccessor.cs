using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Spear.Core.Session
{
    public interface IPrincipalAccessor
    {
        ClaimsPrincipal Principal { get; }
    }

    public static class PrincipalAccessorExtensions
    {
        public static void SetSession(this IPrincipalAccessor accessor, MicroSessionDto session)
        {
            if (accessor?.Principal == null) return;
            var claims = new List<Claim>();
            if (session.UserId != null)
            {
                claims.AddRange(new[]
                {
                    new Claim(MicroClaimTypes.UserId, session.UserId?.ToString()),
                    new Claim(MicroClaimTypes.UserName, session.UserName ?? string.Empty),
                    new Claim(MicroClaimTypes.Role, session.Role ?? string.Empty)
                });
            }
            if (session.TenantId != null)
                claims.Add(new Claim(MicroClaimTypes.TenantId, session.TenantId.ToString()));
            if (!claims.Any()) return;
            var identity = new ClaimsIdentity(claims);
            accessor.Principal.AddIdentity(identity);
        }
    }
}
