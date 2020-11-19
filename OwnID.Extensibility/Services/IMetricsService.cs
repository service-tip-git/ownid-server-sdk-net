using System.Threading.Tasks;

namespace OwnID.Extensibility.Services
{
    public interface IMetricsService
    {
        Task LogStartAsync(string actionName);
        Task LogFinishAsync(string actionName);
        Task LogErrorAsync(string actionName);
        Task LogSwitchAsync(string actionName);
        Task LogCancelAsync(string actionName);
    }
}