namespace PulseMonitor.Worker.Models;

public class Metric
{
    public int Id { get; set; }
    public string DeviceId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public long Timestamp { get; set; }
}
