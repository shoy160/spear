using Acb.Core;
using Acb.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Spear.Tests.Server.Logging
{
    public class NullDisposble : Disposable
    {
        private NullDisposble() { }
        public static NullDisposble Instance => new NullDisposble();
    }

    public class AcbLogger : ILogger
    {
        private readonly Acb.Core.Logging.ILogger _logger;

        public AcbLogger(string category)
        {
            _logger = LogManager.Logger(category);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.Trace(message);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(message);
                    break;
                case LogLevel.Information:
                    _logger.Info(message);
                    break;
                case LogLevel.Warning:
                    _logger.Warn(message);
                    break;
                case LogLevel.Error:
                    _logger.Error(message, exception);
                    break;
                case LogLevel.Critical:
                    _logger.Fatal(message, exception);
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
                return false;
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposble.Instance;
        }
    }
}
