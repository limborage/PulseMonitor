namespace PulseMonitor.Ingestor.Api.Models;

public record HeartbeatMetric(
    string DeviceId,
    double CpuUsage,
    double MemoryUsage,
    long Timestamp
);