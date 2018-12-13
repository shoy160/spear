using Acb.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Logging;
using Spear.Core.Proxy;
using Spear.DotNetty;
using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            LogManager.AddAdapter(new ConsoleAdapter());
            LogManager.LogLevel(LogLevel.Debug);
            var logger = LogManager.Logger<Program>();
            var servers = new ServiceCollection()
                .AddJsonCoder()
                .AddDotNetty()
                .AddConsul("http://192.168.0.252:8500")
                .AddClient();
            var provider = servers.BuildServiceProvider();
            provider.UseClient();
            Console.WriteLine("请输入消息");
            while (true)
            {
                var message = Console.ReadLine();
                Task.Run(async () =>
                {
                    var proxy = provider.GetService<IClientProxy>();
                    var service = proxy.Create<ITestContract>();
                    var t = await service.Get(message);
                    logger.Info(t);
                });
                //var result = CodeTimer.Time("dotnetty test", 100, async () =>
                //{
                //    var proxy = MicroServices.GetService<IClientProxy>();
                //    var service = proxy.Create<ITestContract>();
                //    await service.Get(message);
                //}, 20);
                //logger.Info(result.ToString());
            }
        }
    }
}
