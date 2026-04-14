using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using PulseMonitor.Ingestor.Api.Hubs;
using PulseMonitor.Ingestor.Api.Models;
using System.Text.Json;

namespace PulseMonitor.Ingestor.Api.Services;

public class MetricBroadcaster : BackgroundService
{
    private readonly IHubContext<MetricHub> _hubContext;
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<MetricBroadcaster> _logger;

    public MetricBroadcaster(IHubContext<MetricHub> hubContext, IConfiguration config, ILogger<MetricBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        { 
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Latest,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = config["Kafka:SaslUsername"],
            SaslPassword = config["Kafka:SaslPassword"],
            ApiVersionRequestTimeoutMs = 15000,
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(">>> 📡 BROADCASTER STARTING: Subscribing to health-metrics...");
        _consumer.Subscribe("health-metrics");

        await Task.Yield();
        while (!(stoppingToken.IsCancellationRequested))
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                if (result != null)
                {
                    _logger.LogInformation(">>> 📥 KAFKA RECV: Broadcaster got message for {Key}", result.Message.Key);

                    var metric = JsonSerializer.Deserialize<HeartbeatMetric>(result.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (metric != null)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMetric", metric, stoppingToken);
                        _logger.LogInformation($"Broadcasted metric for {metric.DeviceId}");
                    }

                    _logger.LogInformation(">>> 📤 SIGNALR SENT: Broadcasted to Angular");
                }
            }
            catch (OperationCanceledException) { break; }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning("Waiting for topic 'health-metrics' to be created...");
                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Broadcaster error");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}