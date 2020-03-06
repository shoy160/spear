using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;

namespace Spear.Protocol.WebSocket
{
    [Protocol(ServiceProtocol.Ws)]
    public class WebSocketListener : MicroListener
    {
        private readonly ILogger<WebSocketListener> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageCodecFactory _codecFactory;
        private IHost _host;

        public WebSocketListener(ILoggerFactory loggerFactory, IMessageCodecFactory codecFactory)
        {
            _loggerFactory = loggerFactory;
            _codecFactory = codecFactory;
            _logger = loggerFactory.CreateLogger<WebSocketListener>();
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            //var endpoint = serviceAddress.ToEndPoint() as IPEndPoint;
            _host = Host.CreateDefaultBuilder()
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
            //app.UseMiddleware<WebSocketMiddleware>();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/micro/ws")
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var ms = new MemoryStream();
                    var buffer = new byte[1024 * 4];
                    while (true)
                    {
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                            CancellationToken.None);
                        await ms.WriteAsync(buffer, 0, buffer.Length);
                        if (result.EndOfMessage)
                        {
                            var invokeMessage = await _codecFactory.GetDecoder().DecodeAsync<InvokeMessage>(ms.ToArray());
                            var sender = new WebSocketMessageSender(webSocket, _codecFactory.GetEncoder());
                            await OnReceived(sender, invokeMessage);
                            ms.Dispose();
                            ms = new MemoryStream();
                        }
                        if (result.CloseStatus.HasValue)
                        {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                                CancellationToken.None);
                            _logger.LogInformation($"服务关闭");
                            break;
                        }
                    }

                    return;
                }

                await next();
            });
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            //services.AddTransient<WebSocketMiddleware>();
            //services.AddHostedService<BroadcastTimestamp>();
        }

        public override async Task Stop()
        {
            if (_host != null)
                await _host.StopAsync();
        }
    }
}
