using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;

class RedisCommandInspector
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    private const string CommandQueueKey = "queue:device:command";
    private const string CmdAckStreamKey = "stream:device:cmd:ack";
    private const string TelemtryStreamKey = "stream:device:telemetry";

    public RedisCommandInspector(string redisConnection)
    {
        _redis = ConnectionMultiplexer.Connect(redisConnection);
        _db = _redis.GetDatabase();
    }

    public void Inspect()
    {
        Console.WriteLine("=== 指令队列 ===");
        var queue = _db.ListRange(CommandQueueKey, 0, -1);
        if (queue.Length == 0)
        {
            Console.WriteLine("队列为空");
        }
        else
        {
            for (int i = 0; i < queue.Length; i++)
            {
                Console.WriteLine($"{i}: {queue[i]}");
            }
        }

        Console.WriteLine("\n=== 指令账本（cmd:*） ===");
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);
        var cmdKeys = server.Keys(pattern: "cmd:*").ToArray();

        if (cmdKeys.Length == 0)
        {
            Console.WriteLine("账本为空");
        }
        else
        {
            foreach (var key in cmdKeys)
            {
                var hash = _db.HashGetAll(key);
                Console.WriteLine($"Key: {key}");
                foreach (var entry in hash)
                {
                    Console.WriteLine($"  {entry.Name} = {entry.Value}");
                }
            }
        }

        Console.WriteLine("\n=== ACK Stream (stream:device:cmd:ack) ===");
        var length = _db.StreamLength(CmdAckStreamKey);
        Console.WriteLine($"Stream 长度: {length}");
        var messages = _db.StreamRange(CmdAckStreamKey, "-", "+", count: 10);
        foreach (var msg in messages)
        {
            Console.WriteLine($"ID: {msg.Id}");
            foreach (var nv in msg.Values)
            {
                Console.WriteLine($"  {nv.Name} = {nv.Value}");
            }
        }

        Console.WriteLine("\n=== Telemetry Stream (stream:device:telemetry) ===");
        length = _db.StreamLength(TelemtryStreamKey);
        Console.WriteLine($"Stream 长度: {length}");
        messages = _db.StreamRange(CmdAckStreamKey, "-", "+", count: 10);
        foreach (var msg in messages)
        {
            Console.WriteLine($"ID: {msg.Id}");
            foreach (var nv in msg.Values)
            {
                Console.WriteLine($"  {nv.Name} = {nv.Value}");
            }
        }
    }
}

class Program
{
    static void Main()
    {
        var inspector = new RedisCommandInspector("localhost:6379");
        inspector.Inspect();
    }
}
