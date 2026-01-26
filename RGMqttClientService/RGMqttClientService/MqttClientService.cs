using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;

namespace RGMqttClientService
{
    public class MqttClientService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MqttClientService> _logger;
        private readonly RedisService _redis;

        private IMqttClient _client;
        private MqttClientOptions _options;
        private readonly string _topicRoot;

        public MqttClientService(
            IConfiguration config,
            RedisService redis,
            ILogger<MqttClientService> logger)
        {
            _config = config;
            _logger = logger;
            _redis = redis;
            _topicRoot = _config["Mqtt:TopicRoot"] ?? "qdrgdz";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithClientId(_config["Mqtt:ClientId"] ?? "mqtt-client-service")
                .WithTcpServer(
                _config["Mqtt:BrokerHost"],
                int.Parse(_config["Mqtt:BrokerPort"]))
                  .WithCredentials(
                _config["Mqtt:Username"],
                _config["Mqtt:Password"])
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithCleanSession()
                .Build();

            RegisterEvents();
            await ConnectWithRetryAsync(stoppingToken);

            await Task.WhenAll(
                CommandSendLoopAsync(stoppingToken),
                Task.Delay(Timeout.Infinite, stoppingToken));
        }

        private void RegisterEvents()
        {
            _client.ConnectedAsync += async _ =>
            {
                _logger.LogInformation("MQTT connected");
                await SubscribeAsync();
            };

            _client.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("MQTT disconnected: {Reason}", e.Reason);
                await Task.Delay(3000);
                await ConnectWithRetryAsync(CancellationToken.None);
            };

            _client.ApplicationMessageReceivedAsync += HandleMessageAsync;
        }

        private async Task ConnectWithRetryAsync(CancellationToken token)
        {
            while (!_client.IsConnected && !token.IsCancellationRequested)
            {
                try
                {
                    await _client.ConnectAsync(_options, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MQTT connect failed, retrying...");
                    await Task.Delay(5000, token);
                }
            }
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            if (!TopicParser.TryParse(topic, _topicRoot, out var info))
            {
                _logger.LogWarning("Invalid topic: {Topic}", topic);
                return;
            }

            if (info.Direction == "up")
            {
                if (info.Function == "ack" || info.Function == "response")
                {
                    await HandleCommandAckAsync(info, payload);
                }
                else
                {
                    await HandleTelemetryAsync(info, payload);
                }
            }
        }

        private async Task HandleTelemetryAsync(MqttTopicInfo info, string payload)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("ts", out var t))
                    ts = t.GetInt64();
            }
            catch { }

            await _redis.AddTelemetryAsync(
                info.DeviceId,
                info.Owner,
                info.Catalog,
                info.Function,
                ts,
                payload);
        }

        private async Task HandleCommandAckAsync(MqttTopicInfo info, string payload)
        {
            CommandAck? ack;

            try
            {
                ack = JsonSerializer.Deserialize<CommandAck>(payload);
                if (ack == null || string.IsNullOrEmpty(ack.CommandId))
                    return;
            }
            catch
            {
                return;
            }

            if (await _redis.IsCommandAckedAsync(ack.CommandId))
                return;

            await _redis.MarkCommandAckedAsync(
                ack.CommandId,
                info.DeviceId,
                ack.Result ?? "ok");

            await _redis.AddCommandAckEventAsync(
                ack.CommandId,
                info.DeviceId,
                info.Owner,
                info.Catalog,
                payload);
        }

        private async Task CommandSendLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var cmd = await _redis.PopCommandAsync();
                if (cmd == null)
                {
                    await Task.Delay(500, token);
                    continue;
                }

                var topic = new MqttTopicInfo
                {
                    Owner = cmd.Owner,
                    Catalog = cmd.Catalog,
                    DeviceId = cmd.DeviceId,
                    Direction = "down",
                    Function = cmd.Function
                };

                await PublishAsync(topic, cmd.Payload);
                await _redis.MarkCommandSentAsync(cmd.CommandId);
            }
        }

        public async Task PublishAsync(MqttTopicInfo topic, string payload)
        {
            if (!_client.IsConnected) return;

            var fullTopic =
                $"{_topicRoot}/{topic.Owner}/{topic.Catalog}/{topic.DeviceId}/{topic.Direction}/{topic.Function}";

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(fullTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _client.PublishAsync(message);
        }

        private async Task SubscribeAsync()
        {
            var up = $"{_topicRoot}/+/+/+/up/+";
            await _client.SubscribeAsync(up, MqttQualityOfServiceLevel.AtLeastOnce);
            _logger.LogInformation("Subscribed: {Up}", up);
        }
    }
}
