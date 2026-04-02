using Microsoft.AspNetCore.SignalR;
using PulseMonitor.Ingestor.Api.Models;

namespace PulseMonitor.Ingestor.Api.Hubs;

public class MetricHub : Hub
{
    public async Task SendMetric(HeartbeatMetric metric)
    {
        await Clients.All.SendAsync("ReceiveMetric", metric);
    }
}