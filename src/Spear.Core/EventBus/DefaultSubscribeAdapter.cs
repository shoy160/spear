using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spear.Core.EventBus
{
    public class DefaultSubscribeAdapter : ISubscribeAdapter
    {
        private readonly IConsumeConfigurator _consumeConfigurator;
        private readonly IEnumerable<IEventHandler> _integrationEventHandler;
        private readonly ILogger _logger;
        public DefaultSubscribeAdapter(IConsumeConfigurator consumeConfigurator, IEnumerable<IEventHandler> integrationEventHandler)
        {
            _consumeConfigurator = consumeConfigurator;
            _integrationEventHandler = integrationEventHandler;
            _logger = CurrentIocManager.CreateLogger<DefaultSubscribeAdapter>();
        }

        public void SubscribeAt()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("开启订阅...");
            _consumeConfigurator.Configure(GetQueueConsumers());
        }

        private List<Type> GetQueueConsumers()
        {
            return _integrationEventHandler.Select(consumer => consumer.GetType()).ToList();
        }
    }
}
