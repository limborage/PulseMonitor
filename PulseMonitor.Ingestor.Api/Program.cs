using Confluent.Kafka;
using PulseMonitor.Ingestor.Api.Hubs;
using PulseMonitor.Ingestor.Api.Models;
using PulseMonitor.Ingestor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular", policy => {
        policy.WithOrigins(builder.Configuration["Cors:AngularUrl"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
};

builder.Services.AddSingleton<IProducer<string, string>>(sp => new ProducerBuilder<string, string>(producerConfig).Build());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHostedService<MetricBroadcaster>();

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapHub<MetricHub>("/metricHub");

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
