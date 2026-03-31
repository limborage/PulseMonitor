using Confluent.Kafka;
using PulseMonitor.Ingestor.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
};

builder.Services.AddSingleton<IProducer<string, string>>(sp => new ProducerBuilder<string, string>(producerConfig).Build());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.MapPost("/ingest", async (HeartbeatMetric metric, IProducer<string, string> producer) =>
   {
       var message = new Message<string, string>
       {
           Key = metric.DeviceId,
           Value = System.Text.Json.JsonSerializer.Serialize(metric)
       };

       var deliveryReport = await producer.ProduceAsync("health-metrics", message);

       return Results.Accepted(value: new
       {
           Status = "Sent",
           Partition = deliveryReport.Partition.Value
       });
   }
);

app.Run();
