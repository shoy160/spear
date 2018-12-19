using Acb.Core.Logging;
using Spear.Core.Micro.Services;
using System;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    public class MicroHost : DMicroHost
    {
        private readonly IServiceRegister _serviceRegister;
        private readonly IMicroEntryFactory _entryFactory;
        private readonly ILogger _logger;

        public MicroHost(IMicroExecutor serviceExecutor, IMicroListener microListener, IServiceRegister serviceRegister,
            IMicroEntryFactory entryFactory) : base(serviceExecutor, microListener)
        {
            _serviceRegister = serviceRegister;
            _entryFactory = entryFactory;
            _logger = LogManager.Logger<MicroHost>();
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
                _logger.Info("Micro Host Started");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            var assemblies = _entryFactory.GetServices();
            return _serviceRegister.Regist(assemblies, serviceAddress);
        }

        public override async Task Stop()
        {
            await _serviceRegister.Deregist();
            await MicroListener.Stop();
            _logger.Info("Micro Host Stoped");
        }
    }
}
