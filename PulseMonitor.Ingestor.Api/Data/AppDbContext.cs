using Microsoft.EntityFrameworkCore;
using PulseMonitor.Ingestor.Api.Models;

namespace PulseMonitor.Ingestor.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MetricRecord> Metrics => Set<MetricRecord>();
}
