using WebApiClient.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests
{
    public class PublishConfigRequest : ConfigRequest
    {
        /// <summary>
        /// Configuration content
        /// </summary>
        [AliasAs("content")]
        public string Content { get; set; }

        /// <summary>
        /// Configuration type, options value text, json, xml, yaml, html, properties
        /// </summary>
        [AliasAs("type")]
        public string Type { get; set; } = "json";

        /// <summary>
        /// Configuration application
        /// </summary>
        [AliasAs("appName")]
        public string AppName { get; set; }

        /// <summary>
        /// Configuration tags
        /// </summary>
        [AliasAs("tag")]
        public string Tag { get; set; }
    }
}
