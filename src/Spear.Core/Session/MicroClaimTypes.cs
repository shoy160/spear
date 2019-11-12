using System.Security.Claims;

namespace Spear.Core.Session
{
    public static class MicroClaimTypes
    {
        /// <summary>
        /// UserId.
        /// Default: <see cref="ClaimTypes.Name"/>
        /// </summary>
        public static string UserName { get; set; } = ClaimTypes.Name;

        /// <summary>
        /// UserId.
        /// Default: <see cref="ClaimTypes.NameIdentifier"/>
        /// </summary>
        public static string UserId { get; set; } = ClaimTypes.NameIdentifier;

        /// <summary>
        /// UserId.
        /// Default: <see cref="ClaimTypes.Role"/>
        /// </summary>
        public static string Role { get; set; } = ClaimTypes.Role;

        /// <summary>
        /// TenantId.
        /// Default: http://www.acb.com/identity/claims/tenantId
        /// </summary>
        public static string TenantId { get; set; } = "http://www.acb.com/identity/claims/tenantId";

        public const string HeaderUserAgent = "User-Agent";
        public const string HeaderReferer = "referer";
        public const string HeaderForward = "X-Forwarded-For";
        public const string HeaderRealIp = "X-Real-IP";

        public static string HeaderUserId { get; set; } = "User-Id";
        public static string HeaderUserName { get; set; } = "User-Name";
        public static string HeaderTenantId { get; set; } = "Tenant-Id";
        public static string HeaderRole { get; set; } = "User-Role";
    }
}
