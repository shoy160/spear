using Spear.Core.Tenant;
using System.Linq;

namespace Spear.Core.Session.Impl
{
    public class ClaimMicroSession : AbstractMicroSession
    {
        private readonly IPrincipalAccessor _principalAccessor;

        private readonly ITenantResolver _tenantResolver;

        public ClaimMicroSession(IPrincipalAccessor principalAccessor, ITenantResolver tenantResolver = null)
        {
            _principalAccessor = principalAccessor;
            _tenantResolver = tenantResolver;
        }

        private string GetClaimValue(string type)
        {
            var claim = _principalAccessor.Principal?.Claims.FirstOrDefault(t => t.Type == type);
            return string.IsNullOrWhiteSpace(claim?.Value) ? null : claim.Value;
        }

        public override string UserName =>
            TempSession != null ? TempSession.UserName : GetClaimValue(MicroClaimTypes.UserName);

        public override string Role => TempSession != null ? TempSession.Role : GetClaimValue(MicroClaimTypes.Role);

        protected override object GetUserId()
        {
            return GetClaimValue(MicroClaimTypes.UserId);
        }

        protected override object GetTenantId()
        {
            var tenantId = GetClaimValue(MicroClaimTypes.TenantId);
            if (tenantId != null)
                return tenantId;
            return _tenantResolver?.ResolveTenantId();
        }
    }
}
