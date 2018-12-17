using Acb.Core.Dependency;
using Acb.Core.Extensions;
using Acb.Framework;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Micro;
using Spear.DotNetty;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Server
{
    internal class Program : ConsoleHost
    {
        private static void Main(string[] args)
        {
            MapServiceCollection += services =>
            {
                services.AddMicroService(builder =>
                {
                    builder.AddJsonCoder()
                        .AddDotNettyTcp()
                        .AddConsul("http://192.168.0.252:8500");
                });
            };
            var port = 5002;
            if (args.Length > 0)
                port = args[0].CastTo(port);
            UseServiceProvider += provider => provider.UseMicroService(address =>
            {
                address.Host = "192.168.2.253";
                address.Port = port;
            });
            Start(args);
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => await Shutdown();
            Shutdown().GetAwaiter().GetResult();
        }

        private static async Task Shutdown()
        {
            await CurrentIocManager.Resolve<IMicroHost>().Stop();
            Bootstrap.Dispose();
        }
    }
}
