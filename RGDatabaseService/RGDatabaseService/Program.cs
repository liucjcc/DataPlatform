using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RGDatabaseService;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

try
{
    // ==== 全局日志配置 ====
    builder.Logging.ClearProviders();       // 清除默认日志提供器
    builder.Logging.AddConsole();            // 控制台输出
    builder.Logging.AddFilter(level => level >= LogLevel.Warning); // 全局只显示 Warning/ Error

    // 配置数据库上下文工厂（使用 SQL Server）
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
    {
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            throw new InvalidOperationException("❌ ConnectionString 'Default' 未初始化，请检查 appsettings.json！");
        }
        options.UseSqlServer(connStr);
    });

    // 注册数据库服务
    builder.Services.AddSingleton<DatabaseService>();

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
    builder.Services.AddHostedService<Worker>();


    var host = builder.Build();

    // 数据库初始化/迁移
    using (var scope = host.Services.CreateScope())
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        try
        {
            using var context = factory.CreateDbContext();

            // 开发阶段 直接创建（无迁移历史）
            await context.Database.EnsureCreatedAsync();

            // 使用迁移更新数据库（自动创建数据库和表，如果尚未存在）
            //await context.Database.MigrateAsync();

            // 执行快速查询测试连接
            var canConnect = await context.Database.CanConnectAsync();

            if (canConnect)
            {
                Console.WriteLine("✅ 数据库连接测试成功！");
            }
            else
            {
                Console.WriteLine("⚠️ 数据库连接失败，但未抛出异常。请检查连接字符串。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 数据库连接或迁移失败：{ex.Message}");
            throw; // 根据需求决定是否终止应用
        }
    }
    host.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message );
}


