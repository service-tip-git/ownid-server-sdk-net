using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Extensibility.Services
{
    public interface IMetricsService
    {
        Task LogAsync(string metricName);
    }
}