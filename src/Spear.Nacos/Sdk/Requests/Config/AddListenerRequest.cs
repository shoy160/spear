using Spear.Core.Extensions;
using WebApiClient.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests.Config
{
    public class AddListenerRequest : ConfigRequest
    {
        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        [IgnoreSerialized]
        internal override string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [IgnoreSerialized]
        public override string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [IgnoreSerialized]
        public override string Group { get; set; } = "DEFAULT_GROUP";

        /// <summary>
        /// Configuration value.
        /// </summary>
        /// <value>The content.</value>
        [IgnoreSerialized]
        public string Content { get; set; }

        /// <summary>
        /// A packet field indicating the MD5 value of the configuration. lower
        /// </summary>
        [IgnoreSerialized]
        public string ContentMd5 => Content.Md5().ToLower();

        [AliasAs("Listening-Configs")]
        public string ListeningConfigs => string.IsNullOrWhiteSpace(Tenant)
            ? $"{DataId}{NacosHelper.TwoEncode}{Group}{NacosHelper.TwoEncode}{ContentMd5}{NacosHelper.OneEncode}"
            : $"{DataId}{NacosHelper.TwoEncode}{Group}{NacosHelper.TwoEncode}{ContentMd5}{NacosHelper.TwoEncode}{Tenant}{NacosHelper.OneEncode}";
    }
}
