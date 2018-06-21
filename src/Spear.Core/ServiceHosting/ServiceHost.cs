using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Core.ServiceHosting
{
    public class ServiceHost : IServiceHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostLifetime _hostLifetime;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly List<Action<IServiceProvider>> _serviceMaps;

        public ServiceHost(IServiceProvider serviceProvider, List<Action<IServiceProvider>> serviceMaps)
        {
            _serviceProvider = serviceProvider;
            _hostLifetime = _serviceProvider.GetService<IHostLifetime>();
            _applicationLifetime = _serviceProvider.GetService<IApplicationLifetime>();
            _serviceMaps = serviceMaps;
        }

        public IDisposable Run()
        {
            RunAsync().GetAwaiter().GetResult();
            return this;
        }

        private async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            MapperServices();
            if (_hostLifetime != null)
            {
                await _hostLifetime.WaitForStart(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                _applicationLifetime?.NotifyStarted();
            }
        }

        private void MapperServices()
        {
            if (_serviceProvider == null || _serviceMaps == null)
                return;
            foreach (var map in _serviceMaps)
            {
                map(_serviceProvider);
            }
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
