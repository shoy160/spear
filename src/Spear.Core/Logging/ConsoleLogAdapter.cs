using Acb.Core.Extensions;
using Acb.Core.Logging;
using Acb.Core.Serialize;
using Acb.Core.Timing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Core.Logging
{
    public class ConsoleLog : LogBase
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> LogColors = new Dictionary<LogLevel, ConsoleColor>
        {
            {LogLevel.Trace, ConsoleColor.DarkGray},
            {LogLevel.Debug, ConsoleColor.Gray},
            {LogLevel.Info, ConsoleColor.Cyan},
            {LogLevel.Warn, ConsoleColor.Yellow},
            {LogLevel.Error, ConsoleColor.Red},
            {LogLevel.Fatal, ConsoleColor.Magenta}
        };

        private static void Write(string msg, LogLevel level)
        {
            var hasColor = LogColors.ContainsKey(level);
            if (hasColor)
                Console.ForegroundColor = LogColors[level];
            Console.WriteLine(msg);
            if (hasColor)
                Console.ResetColor();
        }

        /// <summary> 写日志方法 </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            Task.Run(() =>
            {
                if (message != null)
                {
                    if (message.GetType().IsSimpleType())
                    {
                        Write($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]\t{message}", level);
                    }
                    else
                    {
                        Write($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]", level);
                        Write(JsonHelper.ToJson(message, NamingType.CamelCase, true), level);
                    }
                }

                if (exception != null)
                {
                    Write(exception.Format(), level);
                }
            });
        }

        public override bool IsTraceEnabled => true;
        public override bool IsDebugEnabled => true;
        public override bool IsInfoEnabled => true;
        public override bool IsWarnEnabled => true;
        public override bool IsErrorEnabled => true;
        public override bool IsFatalEnabled => true;
    }

    /// <summary> 控制台日志适配器 </summary>
    public class ConsoleAdapter : LoggerAdapterBase
    {
        /// <summary> 创建日志 </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override ILog CreateLogger(string name)
        {
            return new ConsoleLog();
        }
    }
}
