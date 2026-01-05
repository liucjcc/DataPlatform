using DataPlatform.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DataPlatform.Shared.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<DeviceConfigEntity> DeviceList { get; set; }

        public DbSet<DeviceDataEntity> DeviceData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 映射到实际数据库表名
            modelBuilder.Entity<DeviceConfigEntity>().ToTable("DeviceConfig");

            // 默认值
            //modelBuilder.Entity<DeviceConfigEntity>()
            //    .Property(d => d.Status)
            //    .HasDefaultValue(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
