using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Exceptions;
using Spear.Core.Message;
using Spear.Core.Message.Models;
using Spear.Core.Micro;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Filters;
using Spear.Protocol.Http.Sender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spear.Protocol.Http
{
    [Protocol(ServiceProtocol.Http)]
    public class HttpMicroListener : MicroListener, IDisposable
    {
        private readonly IMessageCodec _messageCodec;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly ILogger<HttpMicroListener> _logger;
        private ServiceAddress _address;
        private IHost _host;

        public HttpMicroListener(IMessageCodec messageCodec, IMicroEntryFactory entryFactory, ILoggerFactory loggerFactory)
        {
            _messageCodec = messageCodec;
            _entryFactory = entryFactory;
            _logger = loggerFactory.CreateLogger<HttpMicroListener>();
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            _address = serviceAddress;
            //var endpoint = serviceAddress.ToEndPoint() as IPEndPoint;
            _host = new HostBuilder()
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddFilter("System", level => level >= LogLevel.Warning);
                    builder.AddFilter("Microsoft", level => level >= LogLevel.Warning);
                    builder.AddConsole();
                })
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

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting()
                .AddControllers(options =>
                {
                    //自定义异常捕获
                    options.Filters.Add<DExceptionFilter>();
                })
                .AddControllersAsServices();
        }

        private void BuildApplication(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(routes =>
                {
                    routes.MapGet("micro", async ctx =>
                    {
                        var services = _entryFactory.Entries.ToDictionary(k => $"micro/{k.Key}",
                            v => v.Value.Parameters.ToDictionary(pk => pk.Name,
                                pv => pv.ParameterType.GetTypeInfo().Name));
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(JsonConvert.SerializeObject(services), Encoding.UTF8);
                    });
                    routes.MapGet("healthy", async ctx =>
                    {
                        var header = ctx.Response.GetTypedHeaders();
                        header.CacheControl = new CacheControlHeaderValue { NoCache = true };
                        await ctx.Response.WriteAsync("ok", Encoding.UTF8);
                    });
                    routes.MapPost("micro/executor", async ctx =>
                    {
                        //route.Values.TryGetValue("serviceId", out var serviceId);
                        var sender = new HttpServerMessageSender(_messageCodec, ctx.Response, _address.Gzip);
                        try
                        {
                            await OnReceived(sender, ctx);
                        }
                        catch (Exception ex)
                        {
                            var result = new MessageResult();
                            if (ex is SpearException busi)
                            {
                                result.Code = busi.Code;
                                result.Message = busi.Message;
                            }
                            else
                            {
                                _logger.LogError(ex, ex.Message);
                                result.Code = (int)HttpStatusCode.InternalServerError;
                                result.Message = ex.Message;
                            }

                            await sender.Send(result);
                        }
                    });
                });
        }

        private async Task OnReceived(IMessageSender sender, HttpContext context)
        {
            var input = context.Request.Body;
            if (input.CanSeek)
                input.Seek(0, SeekOrigin.Begin);
            byte[] buffers;
            using (var memstream = new MemoryStream())
            {
                await input.CopyToAsync(memstream);
                buffers = memstream.ToArray();
            }
            var invoke = await _messageCodec.DecodeAsync<InvokeMessage>(buffers, _address.Gzip);
            invoke.Headers ??= new Dictionary<string, string>();
            foreach (var (key, value) in context.Request.Headers)
            {
                invoke.Headers[key] = value;
            }
            await OnReceived(sender, invoke);
        }

        public override Task Stop()
        {
            return _host?.StopAsync();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
