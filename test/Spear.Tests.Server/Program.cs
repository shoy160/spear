using Acb.Core.Extensions;
using Acb.Core.Logging;
using Acb.Core.Serialize;
using Acb.Core.Timing;
using Autofac;
using Spear.Core.Runtime.Server.Impl;
using Spear.DotNetty;
using System;
using System.Net;

namespace Spear.Tests.Server
{
    internal class PRogram
    {
        class ConsoleLog : LogBase
        {
            protected override void WriteInternal(LogLevel level, object message, Exception exception)
            {
                if (message.GetType().IsSimpleType())
                {

                    Console.WriteLine($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]\t{message}");
                }
                else
                {
                    Console.WriteLine($"{Clock.Now:yyyy-MM-dd HH:mm:ss}({LoggerName})[{level}]");
                    Console.WriteLine(JsonHelper.ToJson(message, NamingType.CamelCase, true));
                }
                if (exception != null)
                    Console.WriteLine(exception.Format());
            }

            public override bool IsTraceEnabled { get; } = true;
            public override bool IsDebugEnabled { get; } = true;
            public override bool IsInfoEnabled { get; } = true;
            public override bool IsWarnEnabled { get; } = true;
            public override bool IsErrorEnabled { get; } = true;
            public override bool IsFatalEnabled { get; } = true;
        }

        class LogAdapter : LoggerAdapterBase
        {
            protected override ILog CreateLogger(string name)
            {
                return new ConsoleLog();
            }
        }
        private static void Main(string[] args)
        {
            LogManager.AddAdapter(new LogAdapter());
            var builder = new ServiceBuilder { Services = new ContainerBuilder() };
            builder.UseDotNettyTransport();
            var host = builder.Build();
            using (host)
            {
                host.StartAsync(new IPEndPoint(IPAddress.Any, 5623));
            }

        }
    }
}
