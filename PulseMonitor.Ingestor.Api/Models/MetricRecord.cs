namespace PulseMonitor.Ingestor.Api.Models;

public class MetricRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public DateTime Timestamp { get; set; }
}
