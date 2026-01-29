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

        public DbSet<DeviceEntity> DeviceList { get; set; }

        public DbSet<DeviceDataEntity> DeviceData { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<UserDeviceEntity> UserDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Device ----------
            modelBuilder.Entity<DeviceEntity>(entity =>
            {
                entity.ToTable("Device");

                entity.HasKey(d => d.DeviceId);

                entity.HasIndex(d => d.DeviceId)
                    .IsUnique();

                entity.Property(d => d.DeviceId)
                    .HasMaxLength(64)
                    .IsRequired();
            });

            // ---------- User ----------
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("User");

                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.Username)
                    .IsUnique();

                entity.Property(u => u.Username)
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(u => u.DisplayName)
                    .HasMaxLength(64);

                entity.Property(u => u.Role)
                    .HasMaxLength(32)
                    .IsRequired();

                entity.Property(u => u.IsEnabled)
                    .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ---------- User ⇄ Device ----------
            modelBuilder.Entity<UserDeviceEntity>(entity =>
            {
                entity.ToTable("UserDevice");

                entity.HasKey(ud => ud.Id);

                entity.HasIndex(ud => new { ud.UserId, ud.DeviceId })
                    .IsUnique();

                entity.Property(ud => ud.DeviceId)
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(ud => ud.Permission)
                    .HasConversion<byte>()
                    .IsRequired();

                entity.Property(ud => ud.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ud => ud.User)
                    .WithMany(u => u.UserDevices)
                    .HasForeignKey(ud => ud.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ud => ud.Device)
                    .WithMany()
                    .HasForeignKey(ud => ud.DeviceId)
                    .HasPrincipalKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
