//using Acb.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using Spear.Protocol.Http;
using Spear.Protocol.Tcp;
using Spear.Tests.Contracts;
using Spear.Tests.Server.Services;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Server
{
    internal class Program
    {
        private static IServiceProvider _provider;
        private static void Main(string[] args)
        {
            var port = -1;
            if (args.Length > 0)
                int.TryParse(args[0], out port);
            var protocol = ServiceProtocol.Tcp;
            if (args.Length > 1)
                Enum.TryParse(args[1], out protocol);
            var configBuilder = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true);
            var root = configBuilder.Build();

            var services = new ServiceCollection();
            services.AddMicroService(builder =>
            {
                builder
                    .AddJsonCoder()
                    .AddConsul(root.GetSection("micro:consul").Get<ConsulOption>());
                switch (protocol)
                {
                    case ServiceProtocol.Tcp:
                        builder.AddTcpProtocol();
                        break;
                    case ServiceProtocol.Http:
                        builder.AddHttpProtocol();
                        break;
                }
            });
            services.AddTransient<ITestContract, TestService>();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                //builder.AddAcb();
            });
            _provider = services.BuildServiceProvider();


            _provider.UseMicroService(address =>
            {
                var m = root.GetSection("micro").Get<ServiceAddress>();
                address.Service = m.Service;
                address.Host = m.Host;
                address.Port = port > 80 ? port : m.Port;
                if (address.Port < 80)
                    address.Port = 5000;
            });
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await Shutdown();
                eventArgs.Cancel = true;
            };
            var config = root.GetValue<string>("logLevel");
            _provider.GetService<ILogger<Program>>().LogInformation(config);
            Console.ReadLine();
        }

        private static async Task Shutdown()
        {
            await _provider.GetService<IMicroHost>().Stop();
        }
    }
}
