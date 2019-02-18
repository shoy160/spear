
using Acb.Core.Extensions;
using Acb.Core.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Spear.Tests.Client.Logging
{
    public class DefaultLogger : LogBase
    {
        private readonly ILogger _logger;
        public DefaultLogger(ILogger logger)
        {
            _logger = logger;
        }

        private static LogLevel Convert(Acb.Core.Logging.LogLevel level)
        {
            switch (level)
            {
                case Acb.Core.Logging.LogLevel.Trace:
                    return LogLevel.Trace;
                case Acb.Core.Logging.LogLevel.Debug:
                    return LogLevel.Debug;
                case Acb.Core.Logging.LogLevel.Info:
                    return LogLevel.Information;
                case Acb.Core.Logging.LogLevel.Warn:
                    return LogLevel.Warning;
                case Acb.Core.Logging.LogLevel.Error:
                    return LogLevel.Error;
                case Acb.Core.Logging.LogLevel.Fatal:
                    return LogLevel.Critical;
            }

            return LogLevel.None;
        }
        protected override void WriteInternal(Acb.Core.Logging.LogLevel level, object message, Exception exception)
        {
            var msg = message.GetType().IsSimpleType() ? message.ToString() : JsonConvert.SerializeObject(message);

            _logger.Log(Convert(level), new EventId(0), exception, msg);
        }

        public override bool IsTraceEnabled => _logger.IsEnabled(LogLevel.Trace);
        public override bool IsDebugEnabled => _logger.IsEnabled(LogLevel.Debug);
        public override bool IsInfoEnabled => _logger.IsEnabled(LogLevel.Information);
        public override bool IsWarnEnabled => _logger.IsEnabled(LogLevel.Warning);
        public override bool IsErrorEnabled => _logger.IsEnabled(LogLevel.Error);
        public override bool IsFatalEnabled => _logger.IsEnabled(LogLevel.Critical);
    }
}
