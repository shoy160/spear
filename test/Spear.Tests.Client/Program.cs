using Microsoft.Extensions.DependencyInjection;
using Spear.ProxyGenerator;
using Spear.Tests.Contracts;
using System;

namespace Spear.Tests.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var services = new ServiceCollection()
            //    .AddProxy();
            //var provider = services.BuildServiceProvider();

            //while (true)
            //{
            //    var cmd = Console.ReadLine();
            //    if (cmd == "exit")
            //        break;
            //    var factory = provider.GetService<IProxyFactory>();
            //    var contract = factory.Create<ITestContract>();

            //    var word = contract.Get(cmd).Result;
            //    Console.WriteLine(word);
            //}


            Client.Start(args);
            //TaxiFarePrediction.Start(args);
        }
    }
}
