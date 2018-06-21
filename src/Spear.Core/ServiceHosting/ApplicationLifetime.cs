using Acb.Core.Logging;
using System;
using System.Threading;

namespace Spear.Core.ServiceHosting
{
    public class ApplicationLifetime : IApplicationLifetime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly ILogger _logger;

        public ApplicationLifetime()
        {
            _logger = LogManager.Logger<ApplicationLifetime>();
        }

        public CancellationToken Started => _startedSource.Token;

        public CancellationToken Stopping => _stoppingSource.Token;

        public CancellationToken Stopped => _stoppedSource.Token;

        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred starting the application", ex);
            }
        }


        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred stopping the application", ex);
            }
        }

        public void Stop()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppedSource);
                }
                catch (Exception ex)
                {
                    _logger.Error("An error occurred stopping the application", ex);
                }
            }
        }

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }
            cancel.Cancel(throwOnFirstException: false);
        }
    }
}
