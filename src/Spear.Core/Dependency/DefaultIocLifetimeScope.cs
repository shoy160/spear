using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spear.Core.Dependency
{
    public class DefaultIocLifetimeScope : DefaultIocManager, ILifetimeScope
    {
        private readonly IServiceProvider _provider;

        private readonly IServiceScope _serviceScope;

        public ILifetimeScope BeginLifetimeScope(object tag = null)
        {
            return new DefaultIocLifetimeScope(_provider);
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }

        public DefaultIocLifetimeScope(IServiceProvider provider) : base(provider)
        {
            _serviceScope = provider.CreateScope();
            _provider = provider;
        }
    }
}
