using Microsoft.EntityFrameworkCore;
using PulseMonitor.Worker;
using StackExchange.Redis;
using PulseMonitor.Worker.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var redisConfig = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
