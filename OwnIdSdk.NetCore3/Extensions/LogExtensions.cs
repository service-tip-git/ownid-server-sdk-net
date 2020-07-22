using Microsoft.Extensions.Logging;

namespace OwnIdSdk.NetCore3.Extensions
{
    public static class LogExtensions
    {
        public static void LogWithData(this ILogger logger, LogLevel logLevel, string message, object data)
        {
            using (logger.BeginScope(data))
                logger.Log(logLevel, message);
        }
    }
}