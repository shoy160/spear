using System;
using System.Linq;
using Grpc.Core;
using Grpc.Net.Client;
using Spear.Core;
using Spear.Core.Micro.Services;
using Spear.ProxyGenerator;
using Spear.ProxyGenerator.Impl;
using Spear.ProxyGenerator.Proxy;

namespace Spear.Protocol.Grpc
{
    public class GrpcProxyFactory : ProxyFactory
    {
        private readonly IServiceFinder _finder;

        public GrpcProxyFactory(IResolver resolver, IProxyProvider proxyProvider, AsyncProxyGenerator proxyGenerator,
            IServiceFinder finder) :
            base(resolver, proxyProvider, proxyGenerator)
        {
            _finder = finder;
            //GRpc Http支持
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        protected override object Resolve(Type type, object key = null)
        {
            if (typeof(ClientBase).IsAssignableFrom(type))
            {
                //find service
                var services = _finder.Find(type).ConfigureAwait(false).GetAwaiter().GetResult();
                if (services == null || !services.Any())
                    throw new SpearException("没有可用的服务", 20001);
                var service = services.Random();
                if (service.Protocol != ServiceProtocol.Grpc)
                    return null;
                var channel = GrpcChannel.ForAddress($"http://{service.Service}:{service.Port}", new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Insecure
                });

                return Activator.CreateInstance(type, channel);
            }
            return null;
        }
    }
}
