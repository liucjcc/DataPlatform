using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RGMqttClientService;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RedisService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisService>>();
    var redisService = new RedisService("localhost:6379", logger);
    // 尝试检查连接
    try
    {
        if (!redisService.IsConnected)
        {
            logger.LogError("无法连接 Redis，程序将退出");
            Environment.Exit(-1); // 直接退出程序
        }
        else
        {
            logger.LogInformation("Redis 已成功连接");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Redis 连接检查失败，程序将退出");
        Environment.Exit(-1); // 直接退出程序
    }

    return redisService;
});

builder.Services.AddHostedService<MqttClientService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();





