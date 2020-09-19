using Spear.Core.Tenant;
using System;

namespace Spear.Core.Session.Impl
{
    public abstract class AbstractMicroSession : IMicroSession
    {
        protected SessionDto TempSession { get; private set; }

        public object UserId => TempSession?.UserId ?? GetUserId();

        public object TenantId => TempSession?.TenantId ?? GetTenantId();

        public abstract string UserName { get; }
        public abstract string Role { get; }

        protected abstract object GetUserId();
        protected abstract object GetTenantId();

        public TenancySides TenancySides => TenantId != null
            ? TenancySides.Tenant
            : TenancySides.Host;

        public IDisposable Use(SessionDto sessionDto)
        {
            TempSession = sessionDto;
            return new DisposeAction(() => { TempSession = null; });
        }
    }
}
