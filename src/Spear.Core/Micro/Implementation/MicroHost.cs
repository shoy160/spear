using Microsoft.Extensions.Logging;
using Spear.Core.Micro.Services;
using System;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    public class MicroHost : DMicroHost
    {
        private readonly IServiceRegister _serviceRegister;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly ILogger<MicroHost> _logger;

        public MicroHost(ILogger<MicroHost> logger, IMicroExecutor serviceExecutor, IMicroListener microListener, IServiceRegister serviceRegister,
            IMicroEntryFactory entryFactory) : base(logger, serviceExecutor, microListener)
        {
            _serviceRegister = serviceRegister;
            _entryFactory = entryFactory;
            _logger = logger;
        }

        public override void Dispose()
        {
            (MicroListener as IDisposable)?.Dispose();
        }

        /// <inheritdoc />
        /// <summary> 启动微服务 </summary>
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
                _logger.LogInformation("Micro Host Started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            var assemblies = _entryFactory.GetServices();
            serviceAddress.Protocol = Constants.Protocol;
            return _serviceRegister.Regist(assemblies, serviceAddress);
        }

        public override async Task Stop()
        {
            await _serviceRegister.Deregist();
            await MicroListener.Stop();
            _logger.LogInformation("Micro Host Stoped");
        }
    }
}
