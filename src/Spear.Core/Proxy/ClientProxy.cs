using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Spear.Core.Message.Models;
using Spear.Core.Micro;
using Spear.Core.Micro.Services;
using Spear.Core.Session;
using Spear.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Spear.Core.Proxy
{
    /// <summary> 代理调用 </summary>
    public class ClientProxy : IProxyProvider
    {
        private readonly ILogger<ClientProxy> _logger;

        private readonly IServiceProvider _provider;
        private readonly IServiceFinder _serviceFinder;

        /// <inheritdoc />
        /// <summary> 构造函数 </summary>
        public ClientProxy(ILogger<ClientProxy> logger, IServiceProvider provider, IServiceFinder finder)
        {
            _logger = logger;
            _provider = provider;
            _serviceFinder = finder;
        }

        /// <summary> 执行请求 </summary>
        /// <param name="serviceAddress"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<MessageResult> ClientInvokeAsync(ServiceAddress serviceAddress, InvokeMessage message)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                //获取不同协议的客户端工厂
                var clientFactory = _provider.GetService<IMicroClientFactory>(serviceAddress.Protocol);
                var client = await clientFactory.CreateClient(serviceAddress);
                return await client.Send(message);
            }
            finally
            {
                watch.Stop();
                _logger.LogInformation($"ClientInvokeAsync {watch.ElapsedMilliseconds} ms");
            }
        }

        private async Task<MessageResult> InternalInvoke(MethodInfo targetMethod, IDictionary<string, object> args)
        {
            var serviceType = targetMethod.DeclaringType;

            var services = (await _serviceFinder.Find(serviceType) ?? new List<ServiceAddress>())
                .ToList();
            if (!services.Any())
            {
                throw ErrorCodes.NoService.CodeException();
            }
            var invokeMessage = Create(targetMethod, args);
            ServiceAddress service = null;
            var builder = Policy
                .Handle<Exception>(ex =>
                    ex.GetBaseException() is SocketException ||
                    ex.GetBaseException() is HttpRequestException ||
                    (ex.GetBaseException() is SpearException spearEx && spearEx.Code == ErrorCodes.SystemError)) //服务器异常
                .OrResult<MessageResult>(r => r.Code != 200); //服务未找到

            //熔断,3次异常,熔断5分钟
            var breaker = builder.CircuitBreakerAsync(3, TimeSpan.FromMinutes(5));
            //重试3次
            var retry = builder.RetryAsync(3, (result, count) =>
            {
                _logger.LogWarning(result.Exception != null
                    ? $"{service},{targetMethod.ServiceKey()}:retry,{count},{result.Exception.Format()}"
                    : $"{service},{targetMethod.ServiceKey()}:retry,{count},{result.Result.Code}");
                services.Remove(service);
                _serviceFinder.CleanCache(serviceType);
            });

            var policy = Policy.WrapAsync(retry, breaker);

            return await policy.ExecuteAsync(async () =>
            {
                if (!services.Any())
                {
                    await _serviceFinder.CleanCache(serviceType);
                    throw ErrorCodes.NoService.CodeException();
                }

                service = services.Random();

                return await ClientInvokeAsync(service, invokeMessage);
            });
        }

        private InvokeMessage Create(MethodInfo targetMethod, IDictionary<string, object> args)
        {
            var localIp = Constants.LocalIp();
            var headers = new Dictionary<string, string>
            {
                {SpearClaimTypes.HeaderForward, localIp},
                {SpearClaimTypes.HeaderRealIp, localIp},
                {SpearClaimTypes.HeaderUserAgent, "spear-client"}
                //{MicroClaimTypes.HeaderReferer, string.Empty}
            };
            var session = _provider.GetService<IMicroSession>();
            if (session != null)
            {
                if (session.UserId != null)
                {
                    headers.Add(SpearClaimTypes.HeaderUserId, session.GetUserId<string>());
                    headers.Add(SpearClaimTypes.HeaderUserName,
                        HttpUtility.UrlEncode(session.UserName ?? string.Empty));
                    headers.Add(SpearClaimTypes.HeaderRole, HttpUtility.UrlEncode(session.Role ?? string.Empty));
                }

                if (session.TenantId != null)
                    headers.Add(SpearClaimTypes.HeaderTenantId, session.GetTenantId<string>());
            }
            var serviceId = targetMethod.ServiceKey();
            var invokeMessage = new InvokeMessage
            {
                ServiceId = serviceId,
                Headers = headers,
                Parameters = args
            };
            return invokeMessage;
        }

        public object Invoke(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            var result = InternalInvoke(method, parameters).ConfigureAwait(false).GetAwaiter().GetResult();
            return result.Content;
        }

        public Task InvokeAsync(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            return InternalInvoke(method, parameters);
        }

        public async Task<T> InvokeAsync<T>(MethodInfo method, IDictionary<string, object> parameters, object key = null)
        {
            var result = await InternalInvoke(method, parameters);
            return (T)result.Content;
        }
    }
}
