namespace Spear.Core.Tenant
{
    /// <summary> 租户解析器 </summary>
    public interface ITenantResolver
    {
        /// <summary> 获取租户ID </summary>
        /// <returns></returns>
        object ResolveTenantId();
    }
}
