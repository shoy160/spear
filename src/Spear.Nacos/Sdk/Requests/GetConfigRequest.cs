using WebApiClient.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests
{
    public class GetConfigRequest : ConfigRequest
    {
        [AliasAs("tag")]
        public string Tag { get; set; }
    }
}
