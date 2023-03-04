using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Protocol.WebSocket
{
    [Protocol(ServiceProtocol.Ws)]
    public class WebSocketListener : MicroListener
    {
        private readonly IServiceProvider _hostProvider;
        private IHost _host;
        private ServiceAddress _address;

        public WebSocketListener(IServiceProvider hostProvider)
        {
            _hostProvider = hostProvider;
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            _address = serviceAddress;
            _host = new HostBuilder()
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseKestrel(options =>
                        {
                            options.AllowSynchronousIO = true;
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(65);
                            options.Listen(IPAddress.Any, serviceAddress.Port);
                        })
                        .Configure(BuildApplication);
                })
                .ConfigureServices(ConfigureServices)
                .Build();
            await _host.RunAsync();
        }

        private void BuildApplication(IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketMiddleware>();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddTransient(provider =>
            {
                var lifetime = provider.GetService<IHostApplicationLifetime>();
                var codecFactory = _hostProvider.GetService<IMessageCodec>();
                var loggerFactory = _hostProvider.GetService<ILoggerFactory>();
                return new WebSocketMiddleware(this, lifetime, codecFactory, loggerFactory, _address);
            });
            //services.AddHostedService<BroadcastTimestamp>();
        }

        public override async Task Stop()
        {
            if (_host != null)
                await _host.StopAsync();
        }
    }
}
