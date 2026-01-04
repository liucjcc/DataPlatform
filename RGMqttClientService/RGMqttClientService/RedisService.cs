using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RGMqttClientService
{
    public class RedisService
    {
        // =========================
        // Redis Keys
        // =========================
        private const string CommandQueueKey = "queue:device:command";
        private const string CmdAckStreamKey = "stream:device:cmd:ack";
        private const string TelemetryStreamKey = "stream:device:telemetry";
        private const int CmdAckMaxLength = 1000;
        private const int TelemetryMaxLength = 10 * 1000;

        private readonly string _connectionString;
        private readonly ILogger<RedisService> _logger;
        private ConnectionMultiplexer? _redis;
        private IDatabase? _db;

        public RedisService(string connectionString, ILogger<RedisService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            // 尝试连接 Redis
            ConnectWithRetry();
        }

        // =========================
        // 延迟连接 + 自动重试
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
            Environment.Exit(-1); // 直接退出程序
        }

        // =========================
        // 确保 Redis 已连接
        // =========================
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
        // Telemetry
        // =========================
        public async Task AddTelemetryAsync(
            string deviceId,
            string owner,
            string catalog,
            string function,
            long ts,
            string payload)
        {
            try
            {
                await Db.StreamAddAsync(
                    TelemetryStreamKey,
                    new NameValueEntry[]
                    {
                        new("deviceId", deviceId),
                        new("owner", owner),
                        new("catalog", catalog),
                        new("function", function),
                        new("ts", ts),
                        new("payload", payload)
                    },
                    maxLength: TelemetryMaxLength,
                    useApproximateMaxLength: true
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddTelemetryAsync 出错");
            }
        }

        // =========================
        // Command Queue
        // =========================
        public async Task<DeviceCommand?> PopCommandAsync()
        {
            try
            {
                var value = await Db.ListLeftPopAsync(CommandQueueKey);
                if (value.IsNullOrEmpty) return null;
                return JsonSerializer.Deserialize<DeviceCommand>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PopCommandAsync 出错");
                return null;
            }
        }

        // =========================
        // Command Ledger
        // =========================
        public async Task<bool> IsCommandAckedAsync(string commandId)
        {
            try
            {
                var status = await Db.HashGetAsync($"cmd:{commandId}", "status");
                return status == "ack";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsCommandAckedAsync 出错");
                return false;
            }
        }

        public async Task MarkCommandSentAsync(string commandId)
        {
            try
            {
                var key = $"cmd:{commandId}";
                await Db.HashSetAsync(key,
                    new HashEntry[]
                    {
                        new("status", "sent"),
                        new("sentAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    });
                await Db.KeyExpireAsync(key, TimeSpan.FromHours(2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkCommandSentAsync 出错");
            }
        }

        public async Task MarkCommandAckedAsync(string commandId, string deviceId, string result)
        {
            try
            {
                var key = $"cmd:{commandId}";
                await Db.HashSetAsync(key,
                    new HashEntry[]
                    {
                        new("status", "ack"),
                        new("ackAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                        new("result", result),
                        new("deviceId", deviceId)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkCommandAckedAsync 出错");
            }
        }

        // =========================
        // Command ACK Stream
        // =========================
        public async Task AddCommandAckEventAsync(
            string commandId,
            string deviceId,
            string owner,
            string catalog,
            string payload)
        {
            try
            {
                await Db.StreamAddAsync(
                    CmdAckStreamKey,
                    new NameValueEntry[]
                    {
                        new("cmdId", commandId),
                        new("deviceId", deviceId),
                        new("owner", owner),
                        new("catalog", catalog),
                        new("payload", payload),
                        new("ts", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    },
                    maxLength: CmdAckMaxLength,
                    useApproximateMaxLength: true
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddCommandAckEventAsync 出错");
            }
        }
    }
}
