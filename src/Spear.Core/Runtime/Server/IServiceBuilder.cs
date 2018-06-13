using Autofac;

namespace Spear.Core.Runtime.Server
{
    public interface IServiceBuilder
    {
        ContainerBuilder Services { get; set; }

        IServiceHost Build();
    }
}
