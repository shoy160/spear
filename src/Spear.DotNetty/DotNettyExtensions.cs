using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.ServiceHosting;

namespace Spear.DotNetty
{
    public static class DotNettyExtensions
    {
        public static IServiceHostBuilder UseDotNetty(this IServiceHostBuilder builder)
        {
            var services = builder.Services;
            services.AddSingleton<IMicroClientFactory, DotNettyClientFactory>();
            services.AddSingleton(typeof(DotNettyMicroListener), provider =>
            {
                var coderFactory = provider.GetService<IMessageCoderFactory>();
                return new DotNettyMicroListener(coderFactory);
            });
            services.AddSingleton<IMicroHost>(provider =>
            {
                var listener = provider.GetService<DotNettyMicroListener>();
                var executor = provider.GetService<IMicroExecutor>();
                return new MicroHost(async endPoint =>
                {
                    await listener.Start(endPoint);
                    return listener;
                }, executor);
            });
            return builder;
        }
    }
}
