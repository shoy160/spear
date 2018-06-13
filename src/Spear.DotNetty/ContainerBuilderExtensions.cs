using Autofac;
using Spear.Core.Runtime.Server;
using Spear.Core.Runtime.Server.Impl;
using Spear.Core.Transport;
using Spear.Core.Transport.Impl;
using Spear.DotNetty.Listener;

namespace Spear.DotNetty
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 使用DotNetty进行传输。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseDotNettyTransport(this IServiceBuilder builder)
        {
            var services = builder.Services;
            services.RegisterType(typeof(DotNettyTransportClientFactory)).As(typeof(ITransportClientFactory)).SingleInstance();
            services.RegisterType<JsonMessageCoderFactory>().As<IMessageCoderFactory>().SingleInstance();
            services.RegisterType<DServiceExecutor>().As<IServiceExecutor>().SingleInstance();
            services.Register(provider => new DotNettyMessageListener(provider.Resolve<IMessageCoderFactory>())).SingleInstance();
            services.Register(provider =>
            {
                var messageListener = provider.Resolve<DotNettyMessageListener>();
                var serviceExecutor = provider.Resolve<IServiceExecutor>();
                return new DServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, serviceExecutor);
            }).As<IServiceHost>();
            return builder;
        }
    }
}
