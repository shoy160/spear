using System.Threading.Tasks;
using WebApiClient.Attributes;
using WebApiClient.Contexts;

namespace Spear.Nacos.Sdk.Filters
{
    public class ConfigFilter : ApiActionFilterAttribute
    {
        public override Task OnBeginRequestAsync(ApiActionContext context)
        {
            var config = context.GetService<NacosConfig>();
            context.RequestMessage.AddUrlQuery("tenant", config.Tenant);
            return base.OnBeginRequestAsync(context);
        }
    }
}
