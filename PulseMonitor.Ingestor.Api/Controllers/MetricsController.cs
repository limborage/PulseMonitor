using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using PulseMonitor.Ingestor.Api.Models;
using PulseMonitor.Ingestor.Api.Services;

namespace PulseMonitor.Ingestor.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MetricsController(
    IProducer<string, string> producer,
    IMetricService metricService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(HeartbeatMetric metric)
    {
        var message = new Message<string, string>
        {
            Key = metric.DeviceId,
            Value = System.Text.Json.JsonSerializer.Serialize(metric)
        };

        var deliveryReport = await producer.ProduceAsync("health-metrics", message);

        return Accepted(new
        {
            Status = "Sent",
            Partition = deliveryReport.Partition.Value
        });
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] int count = 10)
    {
        var metrics = await metricService.GetLatestMetricsAsync(count);

        return Ok(metrics);
    }
}
