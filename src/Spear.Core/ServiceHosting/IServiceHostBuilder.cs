using System;
using Microsoft.Extensions.DependencyInjection;

namespace Spear.Core.ServiceHosting
{
    /// <summary> 服务主机构建器 </summary>
    public interface IServiceHostBuilder
    {
        IServiceCollection Services { get; }
        IServiceHost Build();
        IServiceHostBuilder MapServices(Action<IServiceProvider> mapper);
    }
}
