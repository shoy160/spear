using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Spear.Core.Micro
{
    /// <summary> Spear 服务构建器 </summary>
    public interface IMicroBuilder : IServiceCollection
    {
    }

    /// <summary> Spear客户端构建器 </summary>
    public interface IMicroClientBuilder : IMicroBuilder { }

    /// <summary> Spear服务端构建器 </summary>
    public interface IMicroServerBuilder : IMicroBuilder { }

    /// <summary> Spear构建器 </summary>
    public class MicroBuilder : ServiceCollection, IMicroClientBuilder, IMicroServerBuilder
    {
        public MicroBuilder() { }

        public MicroBuilder(IServiceCollection services)
        {
            foreach (var service in services)
            {
                this.Add(service);
            }
        }
    }
}
