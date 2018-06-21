using Acb.Core.Extensions;
using Acb.Core.Logging;
using Acb.Core.Serialize;
using Acb.Core.Timing;
using System;
using System.Collections.Generic;

namespace Spear.Core.Logging
{
    public class ConsoleLog : LogBase
    {
        private static readonly object LockObj = new object();

        private readonly Dictionary<LogLevel, ConsoleColor> _logColors = new Dictionary<LogLevel, ConsoleColor>
        {
            {LogLevel.Trace, ConsoleColor.DarkGray},
            {LogLevel.Debug, ConsoleColor.Gray},
            {LogLevel.Info, ConsoleColor.Cyan},
            {LogLevel.Warn, ConsoleColor.Yellow},
            {LogLevel.Error, ConsoleColor.Red},
            {LogLevel.Fatal, ConsoleColor.Magenta}
        };

        /// <summary> 写日志方法 </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            lock (LockObj)
            {
                if (message != null)
                {
                    if (_logColors.ContainsKey(level))
                        Console.ForegroundColor = _logColors[level];
                    if (message.GetType().IsSimpleType())
                    {

                        Console.WriteLine($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]\t{message}");
                    }
                    else
                    {
                        Console.WriteLine($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]");
                        Console.WriteLine(JsonHelper.ToJson(message, NamingType.CamelCase, true));
                    }
                }

                if (exception != null)
                {
                    Console.WriteLine(exception.Format());
                }
                Console.ResetColor();
            }
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
