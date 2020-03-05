//using Acb.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Config;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using Spear.Nacos;
using Spear.Protocol.Http;
using Spear.Protocol.Tcp;
using Spear.Tests.Contracts;
using Spear.Tests.Server.Services;
using System;
using System.Threading.Tasks;
using Spear.Codec;
using Spear.Codec.ProtoBuffer;

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

            ConfigManager.Instance.UseLocal("_config");

            Console.WriteLine("shay".Config<string>());

            var services = new MicroBuilder();
            services.AddMicroService(builder =>
            {
                builder
                    //.AddJsonCodec()
                    .AddMessagePackCodec()
                    //.AddProtoBufCodec()
                    .AddSession()
                    .AddNacos()
                    .AddConsul()
                    ;
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
            services.AddSingleton<ITestContract, TestService>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
            });
            _provider = services.BuildServiceProvider();
            //_provider.UseNacosConfig();

            _provider.UseMicroService(address =>
            {
                var m = "micro".Config<ServiceAddress>();
                if (m == null) return;
                address.Service = m.Service;
                address.Host = m.Host;
                address.Port = port > 80 ? port : m.Port;
                if (address.Port < 80)
                    address.Port = 5000;
                address.Weight = m.Weight;
            });
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                await Shutdown();
                eventArgs.Cancel = true;
            };
            var config = "oss:keyId".Config<string>();
            _provider.GetService<ILogger<Program>>().LogInformation(config);
            Console.ReadLine();
        }

        private static async Task Shutdown()
        {
            await _provider.GetService<IMicroHost>().Stop();
        }
    }
}
