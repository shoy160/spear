using Acb.Core.Logging;
using Acb.Framework;
using Acb.Framework.Logging;
using Spear.Core;
using Spear.Core.ServiceHosting;
using Spear.DotNetty;
using System.Net;

namespace Spear.Tests.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            LogManager.AddAdapter(new ConsoleAdapter());
            var bootstrap = DBootstrap.Instance;
            bootstrap.Initialize();
            LogManager.SetLevel(LogLevel.All);
            var builder = new ServiceHostBuilder()
                .UseJsonCoder()
                .UseDotNetty()
                .UseServer(new IPEndPoint(IPAddress.Any, 5002));
            using (var host = builder.Build())
            {
                host.Run();
            }
        }
    }
}
