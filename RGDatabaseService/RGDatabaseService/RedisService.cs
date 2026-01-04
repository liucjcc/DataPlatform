using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RGDatabaseService
{
    public class DeviceDataMessage
    {
        public string Owner { get; set; } = "";
        public string Catalog { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Function { get; set; } = "";
        public string Payload { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class CommandResult
    {
        public string CommandId { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Result { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class RedisService
    {
        // =========================
        // Keys & Groups
        // =========================
        private const string TelemetryKey = "stream:device:telemetry";
        private const string CommandAckKey = "stream:device:command:ack";

        private const string TelemetryGroup = "db-group";
        private const string CommandAckGroup = "db-group";

        private readonly string _connectionString;
        private readonly ILogger<RedisService> _logger;
        private ConnectionMultiplexer? _redis;
        private IDatabase? _db;

        public RedisService(string connectionString, ILogger<RedisService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            ConnectWithRetry();
        }

        // =========================
        // 连接 Redis + 自动重试
        // =========================
        private void ConnectWithRetry(int maxRetry = 5, int delayMs = 2000)
        {
            int retry = 0;
            while (retry < maxRetry)
            {
                try
                {
                    _redis = ConnectionMultiplexer.Connect(_connectionString);
                    _db = _redis.GetDatabase();
                    _logger.LogInformation("RedisService 已连接到 {ConnectionString}", _connectionString);
                    return;
                }
                catch (Exception ex)
                {
                    retry++;
                    _logger.LogWarning(ex, "Redis 连接失败 ({Retry}/{MaxRetry})", retry, maxRetry);
                    Thread.Sleep(delayMs);
                }
            }

            _logger.LogError("无法连接 Redis，程序将退出");
            Environment.Exit(-1);
        }

        private void EnsureConnected()
        {
            if (_redis == null || !_redis.IsConnected)
            {
                _logger.LogWarning("Redis 未连接，尝试重新连接...");
                ConnectWithRetry();
            }
        }

        public IDatabase Db
        {
            get
            {
                EnsureConnected();
                return _db!;
            }
        }

        public ConnectionMultiplexer Redis
        {
            get
            {
                EnsureConnected();
                return _redis!;
            }
        }

        public bool IsConnected => _redis?.IsConnected ?? false;

        // =========================
        // Consumer Group Init
        // =========================
        public async Task EnsureGroupAsync(string streamKey, string group)
        {
            try
            {
                await Db.StreamCreateConsumerGroupAsync(streamKey, group, StreamPosition.NewMessages);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // 已存在，忽略
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnsureGroupAsync 出错: {StreamKey}", streamKey);
            }
        }

        // =========================
        // Telemetry Consume
        // =========================
        public async Task<StreamEntry[]> ReadTelemetryAsync(string consumer, int count = 10)
        {
            await EnsureGroupAsync(TelemetryKey, TelemetryGroup);
            try
            {
                return await Db.StreamReadGroupAsync(
                    TelemetryKey,
                    TelemetryGroup,
                    consumer,
                    StreamPosition.NewMessages,
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReadTelemetryAsync 出错");
                return Array.Empty<StreamEntry>();
            }
        }

        public async Task AckTelemetryAsync(RedisValue entryId)
        {
            try
            {
                await Db.StreamAcknowledgeAsync(TelemetryKey, TelemetryGroup, entryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AckTelemetryAsync 出错");
            }
        }

        public DeviceDataMessage ConvertTelemetry(StreamEntry entry)
        {
            string Get(string name) => entry.Values.First(v => v.Name == name).Value!;
            return new DeviceDataMessage
            {
                DeviceId = Get("deviceId"),
                Owner = Get("owner"),
                Catalog = Get("catalog"),
                Function = Get("function"),
                Payload = Get("payload"),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Get("ts"))).UtcDateTime
            };
        }

        // =========================
        // Command ACK Consume
        // =========================
        public async Task<StreamEntry[]> ReadCommandAckAsync(string consumer, int count = 10)
        {
            await EnsureGroupAsync(CommandAckKey, CommandAckGroup);
            try
            {
                return await Db.StreamReadGroupAsync(
                    CommandAckKey,
                    CommandAckGroup,
                    consumer,
                    StreamPosition.NewMessages,
                    count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReadCommandAckAsync 出错");
                return Array.Empty<StreamEntry>();
            }
        }

        public async Task AckCommandAckAsync(RedisValue entryId)
        {
            try
            {
                await Db.StreamAcknowledgeAsync(CommandAckKey, CommandAckGroup, entryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AckCommandAckAsync 出错");
            }
        }

        public CommandResult ParseCommandAck(StreamEntry entry)
        {
            string Get(string name)
            {
                foreach (var v in entry.Values)
                {
                    if ((string)v.Name == name)
                        return !v.Value.IsNull ? (string)v.Value : "";
                }
                return "";
            }

            var tsStr = Get("ts");

            DateTime timestamp =
                long.TryParse(tsStr, out var ts)
                    ? (ts > 9999999999
                        ? DateTimeOffset.FromUnixTimeMilliseconds(ts)
                        : DateTimeOffset.FromUnixTimeSeconds(ts))
                        .UtcDateTime
                    : DateTime.UtcNow;

            return new CommandResult
            {
                CommandId = Get("commandId"),
                DeviceId = Get("deviceId"),
                Result = Get("result"),
                Timestamp = timestamp
            };
        }
    }
}
