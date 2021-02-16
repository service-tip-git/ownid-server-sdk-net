using System;
using Microsoft.Extensions.Logging;

namespace OwnID.Extensibility.Logs
{
    public static class LogExtensions
    {
        public static void LogWithData(this ILogger logger, LogLevel logLevel, string message, object data)
        {
            using (logger.BeginScope(data))
            {
                logger.Log(logLevel, message);
            }
        }

        public static void Log(this ILogger logger, LogLevel logLevel, Func<string> messageGenerator)
        {
            if (!logger.IsEnabled(logLevel))
                return;
            
            logger.Log(logLevel, messageGenerator());
        }
    }
}