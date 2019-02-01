using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Message;
using Spear.Core.Micro.Implementation;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http.Filters;
using Spear.Protocol.Http.Sender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Spear.Protocol.Http
{
    public class HttpMicroListener : MicroListener, IDisposable
    {
        private readonly IMessageCoderFactory _coderFactory;
        private IWebHost _host;

        public HttpMicroListener(IMessageCoderFactory coderFactory)
        {
            _coderFactory = coderFactory;
        }

        public override async Task Start(ServiceAddress serviceAddress)
        {
            var endpoint = serviceAddress.ToEndPoint() as IPEndPoint;
            _host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
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
                //routes.MapGet("micro", async ctx => await MicroServiceRunner.Methods(ctx));
                routes.MapPost("micro", async (request, response, route) =>
                {
                    //route.Values.TryGetValue("contract", out var contract);
                    //route.Values.TryGetValue("method", out var method);
                    var sender = new HttpServerMessageSender(_coderFactory.GetEncoder(), response);
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
                            var logger = app.ApplicationServices.GetService<ILogger<HttpMicroListener>>();
                            logger.LogError(ex, ex.Message);
                            result.Code = (int)HttpStatusCode.InternalServerError;
                            result.Message = ex.Message;
                        }

                        await sender.Send(MicroMessage.CreateResultMessage(Guid.NewGuid().ToString("N"), result));
                    }
                });
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
                input.CopyTo(memstream);
                buffers = memstream.ToArray();
            }
            var message = _coderFactory.GetDecoder().Decode(buffers);
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
