using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core;
using Spear.Core.Message;
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
        private readonly IMessageCodecFactory _codecFactory;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly ILogger<HttpMicroListener> _logger;
        private IWebHost _host;

        public HttpMicroListener(IMessageCodecFactory codecFactory, IMicroEntryFactory entryFactory, ILoggerFactory loggerFactory)
        {
            _codecFactory = codecFactory;
            _entryFactory = entryFactory;
            _logger = loggerFactory.CreateLogger<HttpMicroListener>();
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            var endpoint = serviceAddress.ToEndPoint() as IPEndPoint;
            _host = new WebHostBuilder()
                .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
                .UseKestrel(options =>
                {
                    options.Listen(endpoint);
                })
                .ConfigureServices(ConfigureServices)
                .Configure(AppResolve)
                .Build();
            await _host.RunAsync();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //自定义异常捕获
                options.Filters.Add<DExceptionFilter>();
            }).AddControllersAsServices();
        }

        private void AppResolve(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
                routes.MapGet("micro", async ctx =>
                {
                    var services = _entryFactory.Services.ToDictionary(k => $"micro/{k.Key}",
                        v => v.Value.Parameters.ToDictionary(pk => pk.Name, pv => pv.ParameterType.GetTypeInfo().Name));
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync(JsonConvert.SerializeObject(services), Encoding.UTF8);
                });
                routes.MapPost("micro/executor", async (request, response, route) =>
                {
                    //route.Values.TryGetValue("serviceId", out var serviceId);
                    var sender = new HttpServerMessageSender(_codecFactory.GetEncoder(), response);
                    try
                    {
                        await OnReceived(sender, request);
                    }
                    catch (Exception ex)
                    {
                        var result = new ResultMessage();
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

                        await sender.Send(MicroMessage.CreateResultMessage(Guid.NewGuid().ToString("N"), result));
                    }
                });

                //routes.MapRoute("default", "{controller=Home}/{action=Index}/{Id?}");
            });
        }

        private async Task OnReceived(IMessageSender sender, HttpRequest request)
        {
            var input = request.Body;
            if (input.CanSeek)
                input.Seek(0, SeekOrigin.Begin);
            byte[] buffers;
            using (var memstream = new MemoryStream())
            {
                await input.CopyToAsync(memstream);
                buffers = memstream.ToArray();
            }
            var message = _codecFactory.GetDecoder().Decode(buffers);
            var invoke = message.GetContent<InvokeMessage>();
            invoke.Headers = invoke.Headers ?? new Dictionary<string, string>();
            foreach (var header in request.Headers)
            {
                invoke.Headers[header.Key] = header.Value;
            }
            await OnReceived(sender, message);
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
