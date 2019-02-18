using Acb.Core.Extensions;
using Acb.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Spear.Tests.Client.Logging
{
    public class DefaultAdapter : LoggerAdapterBase
    {
        private readonly IServiceProvider _provider;
        public DefaultAdapter(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override ILog CreateLogger(string name)
        {
            var loggerFactory = _provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(name);
            return new DefaultLogger(logger);
        }
    }
}
