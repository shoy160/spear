using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Contexts;

namespace Spear.Nacos.Sdk.Filters
{
    public class ListenerFilter : IApiParameterAttribute
    {
        public Task BeforeRequestAsync(ApiActionContext context, ApiParameterDescriptor parameter)
        {
            context.RequestMessage.Headers.Add("Long-Pulling-Timeout", "30000");
            return Task.CompletedTask;
        }
    }
}
