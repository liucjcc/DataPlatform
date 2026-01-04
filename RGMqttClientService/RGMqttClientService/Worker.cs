using StackExchange.Redis;

namespace RGMqttClientService
{
    public class Worker : BackgroundService
    {
        private readonly RedisService _redis;

        private readonly ILogger<Worker> _logger;

        public Worker(RedisService redis, ILogger<Worker> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 启动测试模拟器
            var simulator = new CommandTestSimulator(_redis);
            _ = simulator.StartAsync(stoppingToken);
            _ = simulator.ConsumeAckStreamAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
