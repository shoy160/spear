using Microsoft.Extensions.DependencyInjection;

namespace Spear.Core.Micro
{
    /// <summary> Spear 服务构建器 </summary>
    public interface IMicroBuilder
    {
        /// <summary> 服务集合 </summary>
        IServiceCollection Services { get; }
    }

    /// <summary> Spear客户端构建器 </summary>
    public interface IMicroClientBuilder : IMicroBuilder { }

    /// <summary> Spear服务端构建器 </summary>
    public interface IMicroServerBuilder : IMicroBuilder { }

    /// <summary> Spear构建器 </summary>
    public class MicroBuilder : IMicroClientBuilder, IMicroServerBuilder
    {
        public MicroBuilder(IServiceCollection services = null)
        {
            Services = services ?? new ServiceCollection();
        }

        /// <summary> 服务集合 </summary>
        public IServiceCollection Services { get; }
    }
}
