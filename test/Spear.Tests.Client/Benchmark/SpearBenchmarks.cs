using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Spear.Codec.MessagePack;
using Spear.Codec.ProtoBuffer;
using Spear.Consul;
using Spear.Core;
using Spear.Core.Micro;
using Spear.Protocol.Grpc;
using Spear.Protocol.Http;
using Spear.Protocol.Tcp;
using Spear.Protocol.WebSocket;
using Spear.ProxyGenerator;
using Spear.Tests.Contracts;

namespace Spear.Tests.Client.Benchmark
{
    [MemoryDiagnoser]//内存评测
    public class SpearBenchmarks
    {
        private IServiceProvider _provider;
        private ITestContract _contract;
        private Account.AccountClient _client;

        [Params("shay", "123456")]
        public string Name;

        [GlobalSetup]
        public void Setup()
        {
            var services = new MicroBuilder()
                .AddMicroClient(builder =>
                {
                    builder
                        .AddJsonCodec()
                        .AddMessagePackCodec()
                        .AddProtoBufCodec()
                        .AddHttpProtocol()
                        .AddTcpProtocol()
                        .AddWebSocketProtocol()
                        .AddGrpcProtocol()
                        .AddSession()
                        .AddConsul("http://192.168.0.231:8500")
                        //.AddNacos(opt =>
                        //{
                        //    opt.Host = "http://192.168.0.231:8848/";
                        //    opt.Tenant = "ef950bae-865b-409b-9c3b-bc113cf7bf37";
                        //})
                        ;
                });
            _provider = services.BuildServiceProvider();
            var proxy = _provider.GetService<IProxyFactory>();
            //_contract = proxy.Create<ITestContract>();
            _client = proxy.Create<Account.AccountClient>();
        }

        //[Benchmark]
        //public async Task Notice()
        //{
        //    await _contract.Notice(Name);
        //}

        //[Benchmark]
        //public async Task<string> Get()
        //{
        //    var result = await _contract.Get(Name);
        //    return result;
        //    //_provider.GetService<ILogger<SpearBenchmarks>>().LogInformation(result);
        //}

        [Benchmark]
        public async Task<LoginReply> Grpc()
        {
            var result = await _client.LoginAsync(new LoginRequest
            {
                Account = Name
            });
            return result;
            //_provider.GetService<ILogger<SpearBenchmarks>>().LogInformation(result);
        }
    }
}
