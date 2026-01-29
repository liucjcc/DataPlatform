using DataPlatform.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataPlatform.Shared.DataAccess
{
    public class UserRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public UserRepository(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public UserEntity? GetById(long userId)
        {
            using var db = _factory.CreateDbContext();

            return db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == userId);
        }

        public UserEntity? GetByUsername(string username)
        {
            using var db = _factory.CreateDbContext();

            return db.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == username);
        }

        public bool UsernameExists(string username)
        {
            using var db = _factory.CreateDbContext();

            return db.Users
                .Any(u => u.Username == username);
        }

        public void Add(UserEntity user)
        {
            using var db = _factory.CreateDbContext();

            db.Users.Add(user);
            db.SaveChanges();
        }

        public void Update(UserEntity user)
        {
            using var db = _factory.CreateDbContext();

            db.Users.Update(user);
            db.SaveChanges();
        }

        public void Delete(long userId)
        {
            using var db = _factory.CreateDbContext();

            // 查询用户
            var user = db.Users
                .Include(u => u.UserDevices) // 先把关联 UserDevice 加载
                .FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return; // 用户不存在，直接返回

            // 先删除关联的 UserDevice
            if (user.UserDevices.Any())
            {
                db.UserDevices.RemoveRange(user.UserDevices);
            }

            // 删除用户
            db.Users.Remove(user);

            db.SaveChanges();
        }


        public IReadOnlyList<UserDeviceEntity> GetUserDevices(long userId)
        {
            using var db = _factory.CreateDbContext();

            return db.UserDevices
                .AsNoTracking()
                .Where(ud => ud.UserId == userId)
                .ToList();
        }

        public UserDeviceEntity? GetUserDevice(long userId, string deviceId)
        {
            using var db = _factory.CreateDbContext();

            return db.UserDevices
                .FirstOrDefault(ud =>
                    ud.UserId == userId &&
                    ud.DeviceId == deviceId);
        }

        public void AddOrUpdateUserDevice(
            long userId,
            string deviceId,
            DevicePermission permission)
        {
            using var db = _factory.CreateDbContext();

            var entity = db.UserDevices
                .FirstOrDefault(ud =>
                    ud.UserId == userId &&
                    ud.DeviceId == deviceId);

            if (entity is null)
            {
                db.UserDevices.Add(new UserDeviceEntity
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    Permission = permission
                });
            }
            else
            {
                entity.Permission = permission;
                db.UserDevices.Update(entity);
            }

            db.SaveChanges();
        }

        public void RemoveUserDevice(long userId, string deviceId)
        {
            using var db = _factory.CreateDbContext();

            var entity = db.UserDevices
                .FirstOrDefault(ud =>
                    ud.UserId == userId &&
                    ud.DeviceId == deviceId);

            if (entity is null)
                return;

            db.UserDevices.Remove(entity);
            db.SaveChanges();
        }

        public void UpdateUserDevicesDiff(long userId, IReadOnlyList<UserDeviceEntity> newDevices)
        {
            using var db = _factory.CreateDbContext();

            var oldDevices = db.UserDevices
                .Where(x => x.UserId == userId)
                .ToList();

            // 方便查找
            var oldMap = oldDevices.ToDictionary(x => x.DeviceId);
            var newMap = newDevices.ToDictionary(x => x.DeviceId);

            // 1️⃣ 删除：旧有，新没有
            foreach (var old in oldDevices)
            {
                if (!newMap.ContainsKey(old.DeviceId))
                {
                    db.UserDevices.Remove(old);
                }
            }

            // 2️⃣ 新增 / 更新
            foreach (var kv in newMap)
            {
                var deviceId = kv.Key;
                var newItem = kv.Value;

                if (!oldMap.TryGetValue(deviceId, out var oldItem))
                {
                    // 新增
                    db.UserDevices.Add(new UserDeviceEntity
                    {
                        UserId = userId,
                        DeviceId = deviceId,
                        Permission = newItem.Permission
                    });
                }
                else if (oldItem.Permission != newItem.Permission)
                {
                    // 更新
                    oldItem.Permission = newItem.Permission;
                    db.UserDevices.Update(oldItem);
                }
                // 相同则什么都不做
            }

            db.SaveChanges();
        }



    }
}
