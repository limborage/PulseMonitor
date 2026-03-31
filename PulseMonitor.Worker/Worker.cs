namespace PulseMonitor.Worker;
using Confluent.Kafka;
using StackExchange.Redis;
using PulseMonitor.Worker.Data;
using PulseMonitor.Worker.Models;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConsumer<string, string> _consumer;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration config, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _redis = redis;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("health-metrics");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                if (result != null)
                { 
                    var metric = JsonSerializer.Deserialize<Metric>(result.Message.Value, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (metric != null)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        { 
                            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                            db.Metrics.Add(new MetricRecord
                            {
                                DeviceId = metric.DeviceId,
                                CpuUsage = metric.CpuUsage,
                                MemoryUsage = metric.MemoryUsage,
                                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(metric.Timestamp).UtcDateTime
                            });
                            await db.SaveChangesAsync(stoppingToken);

                        }

                        var cache = _redis.GetDatabase();
                        await cache.StringSetAsync($"latest:{metric.DeviceId}", result.Message.Value);

                        _consumer.Commit(result);

                        _logger.LogInformation($"Processed metric from device {metric.DeviceId} at {metric.Timestamp}");
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message. Retrying...");
                await Task.Delay(1000, stoppingToken);
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
