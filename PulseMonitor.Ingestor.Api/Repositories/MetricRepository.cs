using Microsoft.EntityFrameworkCore;
using PulseMonitor.Ingestor.Api.Data;
using PulseMonitor.Ingestor.Api.Models;

namespace PulseMonitor.Ingestor.Api.Repositories;

public class MetricRepository(AppDbContext context) : IMetricRepository
{
    public async Task<List<MetricRecord>> GetLatestAsync(int count)
    {
        return await context.Metrics
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }
}
