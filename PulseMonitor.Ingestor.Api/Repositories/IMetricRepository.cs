using PulseMonitor.Ingestor.Api.Models;

namespace PulseMonitor.Ingestor.Api.Repositories;

public interface IMetricRepository
{
    Task<List<MetricRecord>> GetLatestAsync(int count);
}
