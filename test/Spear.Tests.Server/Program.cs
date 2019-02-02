using Acb.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Micro;
using Spear.Protocol.Http;
using Spear.Tests.Contracts;
using Spear.Tests.Server.Logging;
using Spear.Tests.Server.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Spear.Tests.Server
{
    internal class Program
    {
        private static IServiceProvider _provider;
        private static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddMicroService(builder =>
            {
                builder
                    .AddJsonCoder()
                    .AddHttpProtocol()
                    //.AddTcpProtocol()
                    .AddConsul("http://192.168.0.252:8500");
            });
            services.AddTransient<ITestContract, TestService>();
            services.AddLogging(builder =>
            {
                //builder.AddConsole();
                builder.AddAcb();
            });
            _provider = services.BuildServiceProvider();
            var port = 5002;
            if (args.Length > 0)
                port = args[0].CastTo(port);
            _provider.UseMicroService(address =>
            {
                address.Host = "micro:host".Config<string>(); //"192.168.2.253";
                address.Port = port;
            });
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await Shutdown();
                eventArgs.Cancel = true;
            };
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
            var root = configBuilder.Build();
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
