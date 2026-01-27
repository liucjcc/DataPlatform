using RGDatabaseService;
using System.Collections.Concurrent;

public class Worker : BackgroundService
{
    private readonly RedisService _redis;
    private readonly DatabaseService _dbService;
    private readonly ILogger<Worker> _logger;

    private readonly SemaphoreSlim _signal = new(0);
    private readonly ConcurrentQueue<DeviceDataMessage> _queue = new();

    public Worker( RedisService redis, DatabaseService dbService, ILogger<Worker> logger)
    {
        _redis = redis;
        _dbService = dbService;
        _logger = logger;
    }

    private async Task TelemetryConsumeLoopAsync(CancellationToken token)
    {
        var consumer = Environment.MachineName;

        while (!token.IsCancellationRequested)
        {
            var entries = await _redis.ReadTelemetryAsync(consumer);

            if (entries.Length == 0)
            {
                await Task.Delay(500, token);
                continue;
            }

            foreach (var entry in entries)
            {
                try
                {
                    var msg = _redis.ConvertTelemetry(entry);
                    _queue.Enqueue(msg);
                    _signal.Release();

                    await _redis.AckTelemetryAsync(entry.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telemetry process failed");
                }
            }
        }
    }

    private async Task ProcessDataQueueAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await _signal.WaitAsync(token);

            if (_queue.TryDequeue(out var msg))
            {
                switch (msg.Function)
                {
                    case "data":
                    case "cnemc":
                    case "status":
                    case "alert":
                        await _dbService.SaveDeviceDataAsync(
                            msg.Owner, msg.Catalog, msg.DeviceId,
                            msg.Function, msg.Payload);
                        break;

                    case "command":
                        await _dbService.SaveDeviceCommandAsync(
                            msg.Owner, msg.Catalog, msg.DeviceId,
                            msg.Payload);
                        break;
                }
            }
        }
    }

    private async Task WaitCommandResultLoopAsync(CancellationToken token)
    {
        var consumer = $"{Environment.MachineName}-cmd";

        while (!token.IsCancellationRequested)
        {
            var entries = await _redis.ReadCommandAckAsync(consumer);

            if (entries.Length == 0)
            {
                await Task.Delay(500, token);
                continue;
            }

            foreach (var entry in entries)
            {
                try
                {
                    var result = _redis.ParseCommandAck(entry);

                    _logger.LogInformation(
                        "CommandResult: {Device} {Command} {Result}",
                        result.DeviceId,
                        result.CommandId,
                        result.Result);

                    //await _dbService.SaveDeviceCommandResultAsync(...);

                    await _redis.AckCommandAckAsync(entry.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CommandResult failed");
                }
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            TelemetryConsumeLoopAsync(stoppingToken),
            ProcessDataQueueAsync(stoppingToken),
            WaitCommandResultLoopAsync(stoppingToken)
        );
    }
}
