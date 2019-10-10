using System;
using System.IO;
using BenchmarkDotNet.Running;
using Spear.Tests.Client.Benchmark;

namespace Spear.Tests.Client
{
    public static class Program
    {
        public static string ResolvePath(this string path)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }

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
            //Console.WriteLine("large.json".ResolvePath());
            //var summary = BenchmarkRunner.Run<DeserializeBenchmarks>();
            //Console.ReadLine();
            //Client.Start(args);
            BenchmarkRunner.Run<GrpcClient>();
            //BenchmarkRunner.Run<SpearBenchmarks>();
            //TaxiFarePrediction.Start(args);
        }
    }
}
