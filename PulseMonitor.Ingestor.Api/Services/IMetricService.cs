using PulseMonitor.Ingestor.Api.Models;

namespace PulseMonitor.Ingestor.Api.Services;

public interface IMetricService
{
    Task<List<HeartbeatMetric>> GetLatestMetricsAsync(int count = 10);
}
