using WebApiClient.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests
{
    public class ConfigRequest
    {
        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        [AliasAs("tenant")]
        internal virtual string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [AliasAs("dataId")]
        public virtual string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [AliasAs("group")]
        public virtual string Group { get; set; } = "DEFAULT_GROUP";
    }
}
