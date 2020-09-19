using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Reflection;
using Spear.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Spear.Core.EventBus
{
    public class DefaultConsumeConfigurator : IConsumeConfigurator
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public DefaultConsumeConfigurator(IServiceProvider provider)
        {
            _provider = provider;
            _logger = CurrentIocManager.Resolve<ILoggerFactory>()?.CreateLogger<DefaultConsumeConfigurator>();
        }

        public void Configure(List<Type> consumers)
        {
            foreach (var consumer in consumers)
            {
                if (consumer.GetTypeInfo().IsGenericType)
                {
                    continue;
                }
                var consumerType = consumer.GetInterfaces()
                    .Where(
                        d =>
                            d.GetTypeInfo().IsGenericType &&
                            d.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    .Select(d => d.GetGenericArguments().Single())
                    .First();
                try
                {
                    this.FastInvoke(new[] { consumerType, consumer },
                       x => x.ConsumerTo<object, IEventHandler<object>>());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        protected void ConsumerTo<TEvent, TConsumer>()
            where TConsumer : IEventHandler<TEvent>
            where TEvent : class
        {
            var consumer = CurrentIocManager.Resolve<TConsumer>();
            var name = consumer.GetType().GetCustomAttribute<NamingAttribute>();
            var eventBus = _provider.GetEventBus(name?.Name);
            eventBus.Subscribe<TEvent, TConsumer>(() => consumer);
        }
    }
}
