using Acb.Framework.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spear.Tests.Server.Logging
{
    public class AcbLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AcbLogger(categoryName);
        }
    }

    public static class AcbLoggerExtensions
    {
        public static ILoggingBuilder AddAcb(this ILoggingBuilder builder)
        {
            Acb.Core.Logging.LogManager.AddAdapter(new ConsoleAdapter(), Acb.Core.Logging.LogLevel.All);
            builder.AddProvider(new AcbLoggerProvider());
            return builder;
        }
    }
}
