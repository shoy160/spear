using BenchmarkDotNet.Attributes;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Client.Benchmark
{
    [MemoryDiagnoser]
    public class GrpcClient
    {
        private Greeter.GreeterClient _client;
        [Params("shay", "123456")]
        public string Name;

        [GlobalSetup]
        public void Setup()
        {
            var channel = GrpcChannel.ForAddress(new Uri("https://localhost:5001"));
            _client = new Greeter.GreeterClient(channel);
        }

        [Benchmark]
        public async Task Hello()
        {
            var resp = await _client.SayHelloAsync(new HelloRequest
            {
                Name = "shay"
            });
            //Console.WriteLine(resp.Message);
        }
    }
}
