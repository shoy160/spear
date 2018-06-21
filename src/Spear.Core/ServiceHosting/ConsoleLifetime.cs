using Acb.Core.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Core.ServiceHosting
{
    public class ConsoleLifetime : IHostLifetime
    {
        private readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);
        private readonly ILogger _logger;

        public ConsoleLifetime(IApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            _logger = LogManager.Logger<ConsoleLifetime>();
        }

        public Task WaitForStart(CancellationToken cancellationToken)
        {
            ApplicationLifetime.Started.Register(() =>
            {
                _logger.Info("服务已启动。 按下Ctrl + C关闭。");
            });

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                ApplicationLifetime.Stop();
                _shutdownBlock.WaitOne();
            };
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _shutdownBlock.Set();
                ApplicationLifetime.Stop();
            };
            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _shutdownBlock.Set();
        }
        private IApplicationLifetime ApplicationLifetime { get; }
    }
}
