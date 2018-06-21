using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Micro;
using System;
using System.Collections.Generic;

namespace Spear.Core.ServiceHosting
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        private readonly List<Action<IServiceProvider>> _serviceMaps;
        public IServiceCollection Services { get; }

        public ServiceHostBuilder()
        {
            Services = new ServiceCollection();
            Services.AddSingleton<IApplicationLifetime, ApplicationLifetime>();
            Services.AddSingleton<IHostLifetime, ConsoleLifetime>();
            _serviceMaps = new List<Action<IServiceProvider>>();
        }

        public IServiceHost Build()
        {
            var provider = Services.BuildServiceProvider();
            MicroServices.Current = provider;
            var host = new ServiceHost(provider, _serviceMaps);
            host.Initialize();
            return host;
        }

        public IServiceHostBuilder MapServices(Action<IServiceProvider> mapper)
        {
            if (mapper != null)
                _serviceMaps.Add(mapper);
            return this;
        }
    }
}
