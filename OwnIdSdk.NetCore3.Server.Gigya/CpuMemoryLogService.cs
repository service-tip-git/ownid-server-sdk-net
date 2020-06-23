using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class CpuMemoryLogService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly Process _process = Process.GetCurrentProcess();
        private DateTime _lastTimeStamp;
        private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private TimeSpan _lastUserProcessorTime = TimeSpan.Zero;

        public CpuMemoryLogService(ILogger<CpuMemoryLogService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(LogTelemetry, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void LogTelemetry(object state)
        {
            var totalCpuTimeUsed = _process.TotalProcessorTime.TotalMilliseconds - _lastTotalProcessorTime.TotalMilliseconds;
            var userCpuTimeUsed = _process.UserProcessorTime.TotalMilliseconds - _lastUserProcessorTime.TotalMilliseconds;

            _lastTotalProcessorTime = _process.TotalProcessorTime;
            _lastUserProcessorTime = _process.UserProcessorTime;

            var cpuTimeElapsed = (DateTime.UtcNow - _lastTimeStamp).TotalMilliseconds * Environment.ProcessorCount;
            _lastTimeStamp = DateTime.UtcNow;

            _logger.LogInformation($"total cpu: {totalCpuTimeUsed * 100 / cpuTimeElapsed}");
            _logger.LogInformation($"user cpu: {userCpuTimeUsed * 100 / cpuTimeElapsed}");
            _logger.LogInformation($"WorkingSet64: {_process.WorkingSet64}");
            _logger.LogInformation($"NonpagedSystemMemorySize64: {_process.NonpagedSystemMemorySize64}");
            _logger.LogInformation($"PagedMemorySize64: {_process.PagedMemorySize64}");
            _logger.LogInformation($"PagedSystemMemorySize64: {_process.PagedSystemMemorySize64}");
            _logger.LogInformation($"PrivateMemorySize64: {_process.PrivateMemorySize64}");
            _logger.LogInformation($"VirtualMemorySize64: {_process.VirtualMemorySize64}");
        } 

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}