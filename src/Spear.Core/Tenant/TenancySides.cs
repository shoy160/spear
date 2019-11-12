using System;

namespace Spear.Core.Tenant
{
    /// <summary> 租户站点类型 </summary>
    [Flags]
    public enum TenancySides
    {
        /// <summary> 租户 </summary>
        Tenant = 1,
        /// <summary> 主机 </summary>
        Host = 2
    }
}
