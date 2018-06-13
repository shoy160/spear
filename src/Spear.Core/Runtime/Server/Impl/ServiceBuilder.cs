using Autofac;

namespace Spear.Core.Runtime.Server.Impl
{
    public class ServiceBuilder : IServiceBuilder
    {
        public ContainerBuilder Services { get; set; }
        public IServiceHost Build()
        {
            var container = Services.Build();
            return container.Resolve<IServiceHost>();
        }
    }
}
