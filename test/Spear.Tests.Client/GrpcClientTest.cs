using System;
using Spear.Core.Helper;
using Spear.Core.Serialize;
using Grpc.Core;
using Grpc.Net.Client;
using Spear.Tests.Contracts;

namespace Spear.Tests.Client
{
    public class GrpcClientTest
    {
        public static void Start(params string[] args)
        {
            //GRpc Http支持
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true)
            {
                var msg = Console.ReadLine();
                if (msg == "exit")
                    break;
                var channel = GrpcChannel.ForAddress("http://192.168.2.253:5003", new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Insecure
                });
                var client = new Account.AccountClient(channel);
                var reply = client.Login(new LoginRequest
                {
                    Account = msg,
                    Password = RandomHelper.RandomNums(6),
                    Code = RandomHelper.RandomLetters(4)
                });

                Console.WriteLine(JsonHelper.ToJson(reply));
            }
        }
    }
}
