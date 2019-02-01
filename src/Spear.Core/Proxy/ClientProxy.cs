using Microsoft.Extensions.Logging;
using Polly;
using Spear.Core.Message;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using Spear.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Proxy
{
    /// <summary> 代理调用 </summary>
    public class ClientProxy : IProxyProvider
    {
        private readonly ILogger<ClientProxy> _logger;

        private readonly IMicroClientFactory _clientFactory;
        private readonly IServiceFinder _serviceFinder;

        /// <inheritdoc />
        /// <summary> 构造函数 </summary>
        public ClientProxy(ILogger<ClientProxy> logger, IMicroClientFactory clientFactory, IServiceFinder finder)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _serviceFinder = finder;
        }

        private async Task<ResultMessage> InternalInvoke(MethodInfo targetMethod, IDictionary<string, object> args)
        {
            var services = (await _serviceFinder.Find(targetMethod.DeclaringType) ?? new List<ServiceAddress>()).ToList();
            if (!services.Any())
            {
                throw new SpearException("没有可用的服务");
            }
            var invokeMessage = Create(targetMethod, args);
            ServiceAddress service = null;
            var builder = Policy
                .Handle<Exception>(ex => ex.GetBaseException() is SocketException || ex.GetBaseException() is HttpRequestException) //服务器异常
                .OrResult<ResultMessage>(r => r.Code != 200); //服务未找到
            //熔断,3次异常,熔断5分钟
            var breaker = builder.CircuitBreakerAsync(3, TimeSpan.FromMinutes(5));
            //重试3次
            var retry = builder.RetryAsync(3, (result, count) =>
            {
                _logger.LogWarning(result.Exception != null
                    ? $"{service}{targetMethod.Name}:retry,{count},{result.Exception.Format()}"
                    : $"{service}{targetMethod.Name}:retry,{count},{result.Result.Code}");
                services.Remove(service);
            });

            var policy = Policy.WrapAsync(retry, breaker);

            return await policy.ExecuteAsync(async () =>
            {
                if (!services.Any())
                {
                    throw new SpearException("没有可用的服务");
                }

                service = services.RandomSort().First();

                return await ClientInvokeAsync(service, invokeMessage);
            });
        }

        private static InvokeMessage Create(MethodInfo targetMethod, IDictionary<string, object> args)
        {
            //var remoteIp = AcbHttpContext.RemoteIpAddress;
            //var headers = new Dictionary<string, string>
            //{
            //    {"X-Forwarded-For", remoteIp},
            //    {"X-Real-IP", remoteIp},
            //    {
            //        "User-Agent", AcbHttpContext.Current == null ? "micro_service_client" : AcbHttpContext.UserAgent
            //    },
            //    {"referer", AcbHttpContext.RawUrl}
            //};
            var serviceId = targetMethod.ServiceKey();
            var invokeMessage = new InvokeMessage
            {
                ServiceId = serviceId,
                Parameters = args,
                Headers = new Dictionary<string, string>()
            };
            var type = targetMethod.ReturnType;
            if (type == typeof(void) || type == typeof(Task))
                invokeMessage.IsNotice = true;
            return invokeMessage;
        }

        /// <summary> 执行请求 </summary>
        /// <param name="serviceAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<ResultMessage> ClientInvokeAsync(ServiceAddress serviceAddress, InvokeMessage message)
        {
            //var protocol = serviceAddress.Protocol;
            //:todo 不同的协议处理
            var client = _clientFactory.CreateClient(serviceAddress);
            var result = await client.Send(message);
            return result;
        }

        public object Invoke(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            var result = InternalInvoke(method, parameters).GetAwaiter().GetResult();
            return result.Data;
        }

        public Task InvokeAsync(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            return InternalInvoke(method, parameters);
        }

        public async Task<T> InvokeAsync<T>(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            var result = await InternalInvoke(method, parameters);
            return (T)result.Data;
        }
    }
}
