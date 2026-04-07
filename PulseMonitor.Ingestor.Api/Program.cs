using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using PulseMonitor.Ingestor.Api.Data;
using PulseMonitor.Ingestor.Api.Hubs;
using PulseMonitor.Ingestor.Api.Repositories;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMetricRepository, MetricRepository>();
builder.Services.AddScoped<IMetricService, MetricService>();
builder.Services.AddHostedService<MetricBroadcaster>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAngular");
app.MapHub<MetricHub>("/metricHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
