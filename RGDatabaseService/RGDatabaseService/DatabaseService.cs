using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using static Dapper.SqlMapper;

namespace RGDatabaseService
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        public DbSet<DeviceEntity> DeviceList { get; set; }
        public DbSet<DeviceDataEntity> DeviceData { get; set; }
        public DbSet<CnemcDataEntity> CnemcData { get; set; }
        public DbSet<MqttMessageEntity> DeviceDataQueue { get; set; }
        public DbSet<DeviceCommandEntity> DeviceCommand { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceEntity>().ToTable("Device");
            modelBuilder.Entity<UserEntity>().ToTable("User");
            modelBuilder.Entity<UserDeviceEntity>().ToTable("UserDevice");
            modelBuilder.Entity<DeviceDataEntity>().ToTable("DeviceData");
            modelBuilder.Entity<CnemcDataEntity>().ToTable("CnemcData");
            modelBuilder.Entity<MqttMessageEntity>().ToTable("MqttMessageQueue");
            modelBuilder.Entity<DeviceCommandEntity>().ToTable("DeviceCommand");

            modelBuilder.Entity<DeviceDataEntity>( entity => {
                entity.HasKey(e => e.Id);
                entity.HasIndex(d => new { d.DeviceId, d.Timestamp });
            });

            modelBuilder.Entity<CnemcDataEntity>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasIndex(d => new { d.DeviceId, d.Timestamp });
            });

            modelBuilder.Entity<DeviceCommandEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new {e.DeviceId, e.ReceivedAt});
            });

            modelBuilder.Entity<UserDeviceEntity>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.DeviceId }).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserDevices)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Device)
                      .WithMany()
                      .HasForeignKey(e => e.DeviceId)
                      .OnDelete(DeleteBehavior.Restrict); // 不级联删除设备
            });
        }
    }

    public class DatabaseService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            IDbContextFactory<AppDbContext> dbFactory,
            ILogger<DatabaseService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        // =========================
        // 保存设备数据
        // =========================
        public async Task SaveDeviceDataAsync(
            string owner,
            string catalog,
            string deviceId,
            string functionType,
            string payload)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                if (functionType == "data")
                {
                    db.DeviceData.Add(new DeviceDataEntity
                    {
                        Owner = owner,
                        Catalog = catalog,
                        DeviceId = deviceId,
                        Function = functionType,
                        Payload = payload,
                        Timestamp = DateTime.UtcNow
                    });
                }
                else if (functionType == "cnemc") {
                    db.CnemcData.Add(new CnemcDataEntity
                    {
                        Owner = owner,
                        Catalog = catalog,
                        DeviceId = deviceId,
                        Function = functionType,
                        Payload = payload,
                        Timestamp = DateTime.UtcNow
                    });
                }
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SaveDeviceData failed: {Owner}/{Device}",
                    owner,
                    deviceId);
            }
        }

        // =========================
        // 保存设备指令
        // =========================

        public async Task SaveDeviceCommandAsync(
            string owner,
            string catalog,
            string deviceId,
            string payload)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();

                //var entity = await db.DeviceCommand
                //    .FirstOrDefaultAsync(x =>
                //        x.Owner == owner &&
                //        x.DeviceId == deviceId);

                //if (entity == null)
                //{
                //    entity = new DeviceCommandEntity
                //    {
                //        Owner = owner,
                //        DeviceId = deviceId
                //    };
                //    db.DeviceCommand.Add(entity);
                //}

                var entity = new DeviceCommandEntity
                {
                    Owner = owner,
                    Catalog = catalog,
                    DeviceId = deviceId,
                    Payload = payload,
                    ReceivedAt = DateTime.UtcNow
                };
                db.DeviceCommand.Add(entity);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SaveDeviceCommand failed: {Owner}/{Device}",
                    owner,
                    deviceId);
            }
        }
    }
}

