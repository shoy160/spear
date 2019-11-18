using WebApiClient.DataAnnotations;

namespace Spear.Nacos.Sdk.Requests.Config
{
    public class GetConfigRequest : ConfigRequest
    {
        [AliasAs("tag")]
        public string Tag { get; set; }
    }
}
