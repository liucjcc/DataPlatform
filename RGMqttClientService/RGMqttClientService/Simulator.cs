using System.Text;
using StackExchange.Redis;
using Microsoft.VisualBasic;
using System.Text.Json;

namespace RGMqttClientService
{
    public class CommandTestSimulator
    {
        private const string CommandQueueKey = "queue:device:command";
        private const string CmdAckStreamKey = "stream:device:cmd:ack";
        private const string AckConsumerGroup = "ack-group";    // 消费者组名
        private readonly string _consumerName = Environment.MachineName;
        private readonly IDatabase _db;

        public CommandTestSimulator(RedisService redis)
        {
            _db = redis.Db;
        }

        private async Task EnsureAckConsumerGroupAsync()
        {
            try
            {
                await _db.StreamCreateConsumerGroupAsync(
                    CmdAckStreamKey,
                    AckConsumerGroup,
                    StreamPosition.NewMessages // 从新消息开始消费
                );
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // 消费者组已存在，忽略
            }
        }

        /// <summary>
        /// 模拟消费 ACK Stream 的循环任务（ACK消费者组模式）
        /// </summary>
        public async Task ConsumeAckStreamAsync(CancellationToken token)
        {
            await EnsureAckConsumerGroupAsync();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 使用消费者组读取未处理的消息
                    var entries = await _db.StreamReadGroupAsync(
                        CmdAckStreamKey,
                        AckConsumerGroup,
                        _consumerName,
                        StreamPosition.NewMessages,
                        count: 10
                    );

                    if (entries.Length == 0)
                    {
                        // 没有新消息，稍等
                        await Task.Delay(500, token);
                        continue;
                    }

                    Console.WriteLine("ConsumeAckStreamAsync()----->");

                    foreach (var entry in entries)
                    {
                        try
                        {
                            var cmdId = entry.Values.FirstOrDefault(v => v.Name == "cmdId").Value;
                            var deviceId = entry.Values.FirstOrDefault(v => v.Name == "deviceId").Value;
                            var status = entry.Values.FirstOrDefault(v => v.Name == "status").Value;
                            var result = entry.Values.FirstOrDefault(v => v.Name == "result").Value;

                            Console.WriteLine($"[AckConsumer] CmdId={cmdId}, Device={deviceId}, Status={status}, Result={result}");

                            // TODO: 可更新内部账本或推送前端
                            // await _db.HashSetAsync($"cmd:{cmdId}", new HashEntry[]{...});

                            // ACK：标记消息已处理，消费者组不会重复分发
                            await _db.StreamAcknowledgeAsync(CmdAckStreamKey, AckConsumerGroup, entry.Id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[AckConsumer] Failed to process entry {entry.Id}: {ex}");
                            // 不 ACK → 下次可重试
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AckConsumer] Error: {ex}");
                    await Task.Delay(1000, token);
                }
            }
        }

        /// <summary>
        /// 模拟发送指令与回执
        /// </summary>
        public async Task StartAsync(CancellationToken token)
        {
            int counter = 0;

            while (!token.IsCancellationRequested)
            {
                counter++;
                var cmdId = $"test-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{counter}";

                var cmd = new DeviceCommand
                {
                    CommandId = cmdId,
                    DeviceId = "device-123",
                    Function = "reboot",
                    Payload = "{}",
                    Owner = "user1",
                    Catalog = "cat1",
                    Timestamp = DateTime.UtcNow
                };

                // 写入 Redis 队列
                await _db.ListRightPushAsync(CommandQueueKey, JsonSerializer.Serialize(cmd));

                // 写 Hash 账本
                var cmdKey = $"cmd:{cmd.CommandId}";
                await _db.HashSetAsync(cmdKey, new HashEntry[]
                {
                new("status", "pending"),
                new("deviceId", cmd.DeviceId),
                new("owner", cmd.Owner),
                new("function", cmd.Function),
                new("payload", cmd.Payload),
                new("timestamp", new DateTimeOffset(cmd.Timestamp).ToUnixTimeSeconds())
                });

                Console.WriteLine($"[Test] Command queued: {cmd.CommandId}");

                await Task.Delay(5000, token);
            }
        }
    }
}
