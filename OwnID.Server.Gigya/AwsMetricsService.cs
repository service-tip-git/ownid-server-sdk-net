using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OwnID.Extensibility.Services;
using OwnID.Server.Gigya.Configuration;

namespace OwnID.Server.Gigya
{
    public class AwsMetricsService : IMetricsService, IDisposable
    {
        private const int TimerInterval = 5 * 1000;
        private const int BatchSize = 100;
        private const int UpdateInterval = 60 * 1000;

        private readonly struct Metric
        {
            public DateTime Date { get; }
            public string Name { get; }

            public Metric(DateTime date, string name)
            {
                Date = date;
                Name = name;
            }
        }

        private readonly IOptions<Metrics> _metricsConfiguration;
        private readonly ILogger<AwsMetricsService> _logger;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(UpdateInterval);
        private readonly Timer _timer;
        private DateTime _lastUpdatedTime = DateTime.UtcNow;
        private readonly AmazonCloudWatchClient _amazonCloudWatchClient;

        private readonly ConcurrentQueue<Metric> _loggedData = new ConcurrentQueue<Metric>();

        private bool ShouldProcess =>
            _loggedData.Count != 0
            && (_loggedData.Count > BatchSize || DateTime.UtcNow - _lastUpdatedTime >= _updateInterval);

        public AwsMetricsService(IOptions<AwsConfiguration> awsConfiguration, IOptions<Metrics> metricsConfiguration,
            ILogger<AwsMetricsService> logger)
        {
            _metricsConfiguration = metricsConfiguration;
            _logger = logger;

            _timer = new Timer(TimerInterval)
            {
                AutoReset = true,
                Enabled = true
            };
            _timer.Elapsed += ProcessQueue;

            _amazonCloudWatchClient = new AmazonCloudWatchClient(awsConfiguration.Value.AccessKeyId,
                awsConfiguration.Value.SecretAccessKey,
                RegionEndpoint.GetBySystemName(awsConfiguration.Value.Region));
        }

        private readonly object _processQueueSync = new object();

        private void ProcessQueue(object sender, ElapsedEventArgs e)
        {
            if (!ShouldProcess)
                return;

            lock (_processQueueSync)
            {
                if (!ShouldProcess)
                    return;

                try
                {
                    _lastUpdatedTime = DateTime.UtcNow;

                    var sendBatchTasks = new List<Task>();
                    var process = true;
                    while (process)
                    {
                        var items = new List<Metric>(BatchSize);
                        for (var i = 0; i < BatchSize; i++)
                        {
                            if (!_loggedData.TryDequeue(out var item))
                            {
                                process = false;
                                break;
                            }

                            items.Add(item);
                        }

                        sendBatchTasks.Add(SendBatchAsync(items));
                    }

                    Task.WaitAll(sendBatchTasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        private async Task SendBatchAsync(IList<Metric> items)
        {
            if (!items.Any())
                return;

            var metrics = new Dictionary<Metric, int>();
            foreach (var item in items)
            {
                if (!metrics.ContainsKey(item))
                    metrics.Add(item, 0);

                metrics[item]++;
            }

            var awsMetrics = metrics.Keys.Select(key => new MetricDatum
            {
                MetricName = key.Name,
                TimestampUtc = key.Date,
                Unit = StandardUnit.Count,
                Value = metrics[key]
            }).ToList();

            await SendDataToAwsAsync(awsMetrics);
        }

        private async Task SendDataToAwsAsync(List<MetricDatum> metrics)
        {
            var request = new PutMetricDataRequest
            {
                MetricData = metrics,
                Namespace = _metricsConfiguration.Value.Namespace
            };

            try
            {
                await _amazonCloudWatchClient.PutMetricDataAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }

        private static DateTime RoundToMinute(DateTime dt)
        {
            return RoundUp(dt, TimeSpan.FromMinutes(1));
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _amazonCloudWatchClient?.Dispose();
        }

        public Task LogStartAsync(string actionName)
        {
            return LogAsync(actionName);
        }

        public Task LogFinishAsync(string actionName)
        {
            return LogAsync($"{actionName} success");
        }

        public Task LogErrorAsync(string actionName)
        {
            return LogAsync($"{actionName} error");
        }

        public Task LogSwitchAsync(string actionName)
        {
            return LogAsync($"{actionName} switched");
        }

        public Task LogCancelAsync(string actionName)
        {
            return LogAsync($"{actionName} canceled");
        }

        private Task LogAsync(string metricName)
        {
            _loggedData.Enqueue(new Metric(RoundToMinute(DateTime.UtcNow), metricName));
            return Task.CompletedTask;
        }
    }
}