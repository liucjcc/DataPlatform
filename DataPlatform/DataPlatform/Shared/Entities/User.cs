using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPlatform.Shared.Entities
{
    public enum DevicePermission : byte
    {
        Read = 1,   // 只能看数据
        Control = 2,   // 下发命令
        Manage = 3    // 参数、校准、绑定
    }

    public class UserDeviceEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DeviceId { get; set; }

        [Required]
        public DevicePermission Permission { get; set; }
            = DevicePermission.Read;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // —— 导航属性 ——
        [ForeignKey(nameof(UserId))]
        public UserEntity User { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public DeviceEntity Device { get; set; }
    }

    public class UserEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Username { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; }

        [MaxLength(64)]
        public string? DisplayName { get; set; }

        [MaxLength(32)]
        public string Role { get; set; } = "user";     // admin / user / readonly

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // —— 导航属性 ——
        public ICollection<UserDeviceEntity> UserDevices { get; set; }
            = new List<UserDeviceEntity>();
    }
}
