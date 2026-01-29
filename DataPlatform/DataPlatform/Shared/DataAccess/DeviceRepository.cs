
using DataPlatform.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using DataPlatform.Utils;

namespace DataPlatform.Shared.DataAccess
{
    public class DeviceRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public DeviceRepository(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public bool DeviceExists(string deviceId)
        {
            using var db = _factory.CreateDbContext();
            return db.DeviceList.Any(d => d.DeviceId == deviceId);
        }

        public List<DeviceEntity> GetAllDevices()
        {
            using var db = _factory.CreateDbContext();
            return db.DeviceList
                     .OrderBy(d => d.DeviceName)
                     .ToList();// ToList() 会立即执行查询
        }

        public DeviceEntity GetDeviceById(string deviceId)
        {
            using var db = _factory.CreateDbContext();
            var device = db.DeviceList.Find(deviceId);

            if (device == null)
                throw new InvalidOperationException($"DeviceId {deviceId} 不存在");

            return device;
        }

        public void AddDevice(DeviceEntity device)
        {
            using var db = _factory.CreateDbContext();

            if (db.DeviceList.Any(d => d.DeviceId == device.DeviceId))
                throw new InvalidOperationException("DeviceId 已存在");

            db.DeviceList.Add(device);
            db.SaveChanges();
        }

        public DeviceEntity DeleteDevice(string deviceId)
        {
            using var db = _factory.CreateDbContext();

            var existing = db.DeviceList.Find(deviceId);
            if (existing == null)
                throw new InvalidOperationException("DeviceId 不存在");

            var deletedDevice = new DeviceEntity
            {
                DeviceId = existing.DeviceId,
                DeviceName = existing.DeviceName,
                Status = existing.Status,
                LastOnlineTime = existing.LastOnlineTime
            };

            db.DeviceList.Remove(existing);
            db.SaveChanges();

            return deletedDevice;
        }

        public void UpdateDevice(DeviceEntity device)
        {
            using var db = _factory.CreateDbContext();

            var existing = db.DeviceList.Find(device.DeviceId);
            if (existing == null)
                throw new InvalidOperationException("DeviceId 不存在");

            existing.DeviceName = device.DeviceName;
            db.SaveChanges();
        }

        public void MarkDeviceOnline(string deviceId)
        {
            using var db = _factory.CreateDbContext();
            var device = db.DeviceList.Find(deviceId);

            if (device == null)
                throw new InvalidOperationException("DeviceId 不存在");

            device.LastOnlineTime = DateTime.Now;
            device.Status = true;
            db.SaveChanges();
        }

        public void MarkOfflineDevices(TimeSpan offlineThreshold)
        {
            using var db = _factory.CreateDbContext();
            var cutoff = DateTime.Now - offlineThreshold;
            var offlineDevices = db.DeviceList
                                   .Where(d => d.LastOnlineTime < cutoff && d.Status == true)
                                   .ToList();

            foreach (var d in offlineDevices)
            {
                d.Status = false;
            }

            db.SaveChanges();
        }

        // 2️⃣ 获取指定 deviceId 的最新一条数据
        public DeviceDataEntity GetLatestData(string deviceId)
        {
            using var db = _factory.CreateDbContext();
            var latest = db.DeviceData
                .Where(d => d.DeviceId == deviceId)
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefault();

            if (latest == null)
                throw new InvalidOperationException($"DeviceId {deviceId} 没有数据");

            latest.Timestamp = DateTimeHelper.ToSafeLocalTime(latest.Timestamp)!.Value;

            return latest;
        }

        public List<DeviceDataEntity> GetDeviceData(string deviceId, DateTime t1, DateTime t2)
        {
            using var db = _factory.CreateDbContext();
            var dataList = db.DeviceData
                .Where(d => d.DeviceId == deviceId && d.Timestamp >= t1 && d.Timestamp < t2)
                .ToList();

            if (dataList.Count == 0)
                throw new InvalidOperationException($"DeviceId {deviceId} 在 {t1} 到 {t2} 之间没有数据");

            return dataList;
        }
    }
}
