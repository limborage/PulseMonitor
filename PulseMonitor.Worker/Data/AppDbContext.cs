using Microsoft.EntityFrameworkCore;

namespace PulseMonitor.Worker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MetricRecord> Metrics => Set<MetricRecord>();
}

public class MetricRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public DateTime Timestamp { get; set; }
}