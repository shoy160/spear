using Acb.Core.Logging;
using Spear.Core;
using Spear.Core.Logging;
using Spear.Core.Proxy;
using Spear.Core.ServiceHosting;
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
            LogManager.SetLevel(LogLevel.Info);
            var logger = LogManager.Logger<Program>();
            var builder = new ServiceHostBuilder()
                .UseJsonCoder()
                .UseDotNetty()
                .UseClient();
            var host = builder.Build();
            while (true)
            {
                var message = Console.ReadLine();
                Task.Run(async () =>
                {
                    var proxy = MicroServices.GetService<IClientProxy>();
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
