using Microsoft.Extensions.Logging;
using Spear.Core.Micro.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    /// <summary> 服务宿主 </summary>
    public class MicroHost : DMicroHost
    {
        private readonly IServiceRegister _serviceRegister;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly ILogger<MicroHost> _logger;
        private readonly ServiceProtocol _protocol;

        public MicroHost(IMicroExecutor serviceExecutor, IMicroListener microListener,
            IServiceRegister serviceRegister, IMicroEntryFactory entryFactory, ILoggerFactory loggerFactory)
            : base(serviceExecutor, microListener, loggerFactory)
        {
            _serviceRegister = serviceRegister;
            _entryFactory = entryFactory;
            _logger = loggerFactory.CreateLogger<MicroHost>();
            var protocol = microListener.GetType().GetCustomAttribute<ProtocolAttribute>();
            if (protocol != null)
                _protocol = protocol.Protocol;
        }

        public override void Dispose()
        {
            (MicroListener as IDisposable)?.Dispose();
        }

        /// <inheritdoc />
        /// <summary> 启动服务 </summary>
        /// <param name="serviceAddress">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public override Task Start(ServiceAddress serviceAddress)
        {
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    await MicroListener.Start(serviceAddress);
                });
                Console.WriteLine($"Service Start At {serviceAddress}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            var assemblies = _entryFactory.GetContracts();
            serviceAddress.Protocol = _protocol;
            return _serviceRegister.Regist(assemblies, serviceAddress);
        }

        /// <summary> 停止服务 </summary>
        /// <returns></returns>
        public override async Task Stop()
        {
            await _serviceRegister.Deregist();
            await MicroListener.Stop();
            Console.WriteLine("Service Stoped");
        }
    }
}
