using PulseMonitor.Ingestor.Api.Models;
using PulseMonitor.Ingestor.Api.Repositories;

namespace PulseMonitor.Ingestor.Api.Services;

public class MetricService(IMetricRepository repository) : IMetricService
{
    public async Task<List<HeartbeatMetric>> GetLatestMetricsAsync(int count = 10)
    {
        var records = await repository.GetLatestAsync(count);

        return records.Select(r => new HeartbeatMetric(
            r.DeviceId,
            r.CpuUsage,
            r.MemoryUsage,
            new DateTimeOffset(r.Timestamp).ToUnixTimeMilliseconds()
        )).ToList();
    }
}
