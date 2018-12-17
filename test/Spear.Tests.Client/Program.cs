using Acb.Core.Extensions;
using Acb.Core.Logging;
using Acb.Core.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Logging;
using Spear.Core.Proxy;
using Spear.DotNetty;
using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;
using Acb.Core.Serialize;

namespace Spear.Tests.Client
{
    internal class Program
    {

        public interface IService
        {
            string Name(string name);
        }

        [Naming("serviceA")]
        public class ServieA : IService
        {
            public string Name(string name)
            {
                return $"service A : {name}";
            }
        }

        [Naming("serviceB")]
        public class ServieB : IService
        {
            public string Name(string name)
            {
                return $"service B : {name}";
            }
        }

        private static void Main(string[] args)
        {
            LogManager.ClearAdapter();
            LogManager.LogLevel(LogLevel.Info);
            LogManager.AddAdapter(new ConsoleAdapter());

            var logger = LogManager.Logger<Program>();
            var services = new ServiceCollection()
                .AddMicroClient(opt =>
                {
                    opt.AddJsonCoder()
                        .AddDotNettyClient()
                        .AddConsul("http://192.168.0.252:8500");
                });
            services.AddSingleton<IService, ServieA>();
            services.AddSingleton<IService, ServieB>();
            var provider = services.BuildServiceProvider();
            provider.UseMicroClient();
            logger.Info("请输入消息");
            while (true)
            {
                var message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message))
                    continue;
                var t = provider.GetService<IService>("serviceA");
                Console.WriteLine(t.Name(message));
                var msgArgs = message.Split(',');
                int repeat = 1, thread = 1;
                var isNotice = true;
                if (msgArgs.Length > 1)
                    repeat = msgArgs[1].CastTo(1);
                if (msgArgs.Length > 2)
                    thread = msgArgs[2].CastTo(1);
                if (msgArgs.Length > 3)
                    isNotice = msgArgs[3].CastTo(true);
                message = msgArgs[0];
                Task.Run(() =>
                {
                    var proxy = provider.GetService<IClientProxy>();
                    var service = proxy.Create<ITestContract>();

                    var result = CodeTimer.Time("dotnetty test", repeat, () =>
                    {
                        if (isNotice)
                        {
                            service.Notice(message).Wait();
                        }
                        else
                        {
                            var msg = service.Get(message).Result;
                        }
                    }, thread);
                    logger.Info(result.ToString());
                });
            }
        }
    }
}
