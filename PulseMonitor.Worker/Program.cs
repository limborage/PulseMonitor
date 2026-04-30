using Microsoft.EntityFrameworkCore;
using PulseMonitor.Worker;
using StackExchange.Redis;
using PulseMonitor.Worker.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var redisConfig = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

var options = ConfigurationOptions.Parse(redisConfig);
options.AbortOnConnectFail = false;
options.ConnectTimeout = 5000;

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

host.Run();
