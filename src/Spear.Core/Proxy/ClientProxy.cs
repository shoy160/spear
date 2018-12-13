using Acb.Core;
using Acb.Core.Exceptions;
using Acb.Core.Extensions;
using Acb.Core.Logging;
using Polly;
using Spear.Core.Message;
using Spear.Core.Message.Implementation;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Proxy
{
    /// <summary> 代理调用 </summary>
    public class ClientProxy : ProxyAsync, IClientProxy
    {
        private readonly ILogger _logger;

        private IMicroClientFactory _clientFactory;
        private readonly IServiceFinder _serviceFinder;

        /// <inheritdoc />
        /// <summary> 构造函数 </summary>
        public ClientProxy()
        {
            _logger = LogManager.Logger<ClientProxy>();
            _clientFactory = ClientContext.Resolve<IMicroClientFactory>();
            _serviceFinder = ClientContext.Resolve<IServiceFinder>();
        }

        private async Task<ResultMessage> BaseInvoke(MethodInfo targetMethod, object[] args)
        {
            var services = (await _serviceFinder.Find(targetMethod.DeclaringType) ?? new List<ServiceAddress>()).ToList();
            var invokeMessage = Create(targetMethod, args);
            ServiceAddress service = null;
            var builder = Policy
                .Handle<Exception>(ex => ex.GetBaseException() is SocketException) //服务器异常
                .OrResult<ResultMessage>(r => r.Code != 200); //服务未找到
            //熔断,3次异常,熔断5分钟
            var breaker = builder.CircuitBreakerAsync(3, TimeSpan.FromMinutes(5));
            //重试3次
            var retry = builder.RetryAsync(3, (result, count) =>
            {
                _logger.Warn(result.Exception != null
                    ? $"{service}{targetMethod.Name}:retry,{count},{result.Exception.Format()}"
                    : $"{service}{targetMethod.Name}:retry,{count},{result.Result.Code}");
                services.Remove(service);
            });

            var policy = Policy.WrapAsync(retry, breaker);

            return await policy.ExecuteAsync(async () =>
            {
                if (!services.Any())
                {
                    throw ErrorCodes.NoService.CodeException();
                }

                service = services.RandomSort().First();

                return await InvokeAsync(service.ToEndPoint(), invokeMessage);
            });
        }

        /// <summary> 接口调用 </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object Invoke(MethodInfo method, object[] args)
        {
            var result = BaseInvoke(method, args);
            return result.Result.Data;
        }

        public override Task InvokeAsync(MethodInfo method, object[] args)
        {
            return BaseInvoke(method, args);
        }

        public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            var result = await BaseInvoke(method, args);
            return (T)result.Data;
        }

        private static InvokeMessage Create(MethodInfo targetMethod, object[] args)
        {
            var remoteIp = AcbHttpContext.RemoteIpAddress;
            var headers = new Dictionary<string, string>
            {
                {"X-Forwarded-For", remoteIp},
                {"X-Real-IP", remoteIp},
                {
                    "User-Agent", AcbHttpContext.Current == null ? "micro_service_client" : AcbHttpContext.UserAgent
                },
                {"referer", AcbHttpContext.RawUrl}
            };
            var methodType = targetMethod.DeclaringType;
            var serviceId = $"{methodType?.FullName}.{targetMethod.Name}";
            var dict = new Dictionary<string, object>();
            var parameters = targetMethod.GetParameters();
            if (parameters.Any())
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    dict.Add(parameters[i].Name, args[i]);
                }

                serviceId += "_" + string.Join("_", parameters.Select(i => i.Name));
            }

            var invokeMessage = new InvokeMessage
            {
                ServiceId = serviceId,
                Parameters = dict,
                Headers = headers
            };
            var type = targetMethod.ReturnType;
            if (type == typeof(void) || type == typeof(Task))
                invokeMessage.IsNotice = true;
            return invokeMessage;
        }

        /// <summary> 执行请求 </summary>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<ResultMessage> InvokeAsync(EndPoint endPoint, InvokeMessage message)
        {
            var client = _clientFactory.CreateClient(endPoint);
            var result = await client.Send(message);
            return result;
        }

        public T Create<T>()
        {
            return Create<T, ClientProxy>();
        }

        public void SetClient(IMicroClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
    }
}
