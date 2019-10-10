using Acb.Core.Extensions;
using Acb.Core.Logging;
using Acb.Core.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Consul;
using Spear.Core;
using Spear.Protocol.Http;
using Spear.Protocol.Tcp;
using Spear.ProxyGenerator;
using Spear.Tests.Client.Logging;
using Spear.Tests.Client.Services;
using Spear.Tests.Client.Services.Impl;
using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Spear.Tests.Client
{
    public class Client
    {
        public static void Start(params string[] args)
        {
            var services = new ServiceCollection()
                .AddMicroClient(builder =>
                {
                    builder.AddJsonCoder()
                        .AddHttpProtocol()
                        .AddTcpProtocol()
                        .AddConsul("http://192.168.0.231:8500");
                });
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddConsole();
            });
            services.AddSingleton<DefaultAdapter>();
            services.AddSingleton<IService, ServieA>();
            services.AddSingleton<IService, ServieB>();
            var provider = services.BuildServiceProvider();

            LogManager.AddAdapter(provider.GetService<DefaultAdapter>());
            LogManager.Logger<Client>().Info("test");

            var logger = provider.GetService<ILogger<Client>>();
            logger.LogInformation("请输入消息");
            while (true)
            {
                var message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message))
                    continue;
                var msgArgs = message.Split(new[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries);
                int repeat = 1, thread = 1;
                var isNotice = true;
                if (msgArgs.Length > 1)
                    repeat = msgArgs[1].CastTo(1);
                if (msgArgs.Length > 2)
                    thread = msgArgs[2].CastTo(1);
                if (msgArgs.Length > 3)
                    isNotice = msgArgs[3].CastTo(true);
                message = msgArgs[0];
                //var watch = Stopwatch.StartNew();
                //var proxy = provider.GetService<IProxyFactory>();
                //var service = proxy.Create<ITestContract>();
                //if (isNotice)
                //{
                //    service.Notice(message).GetAwaiter().GetResult();
                //}
                //else
                //{
                //    var msg = service.Get(message).Result;
                //    logger.Debug(msg);
                //}
                //watch.Stop();
                //logger.Info(watch.ElapsedMilliseconds);
                Task.Run(async () =>
                {
                    var proxy = provider.GetService<IProxyFactory>();
                    var service = proxy.Create<ITestContract>();

                    var result = await CodeTimer.Time("micro test", repeat, async () =>
                    {
                        try
                        {
                            if (isNotice)
                            {
                                await service.Notice(message);
                            }
                            else
                            {
                                var msg = await service.Get(message);
                                logger.LogInformation(msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, ex.Message);
                            throw;
                        }
                    }, thread);
                    Console.WriteLine(result.ToString());
                });
            }
        }
    }
}
