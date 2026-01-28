using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGDatabaseService
{
    //    分区存储策略
    //    ALTER TABLE DeviceData
    //    ADD PartitionKey AS
    //    (CASE Function
    //        WHEN 'data' THEN 100000 + YEAR(Timestamp)*100 + MONTH(Timestamp)
    //        WHEN 'status' THEN 200000 + YEAR(Timestamp)*100 + MONTH(Timestamp)
    //    END);

    //public class MqttTopicInfo
    //{
    //    public string Owner { get; set; }
    //    public string Catalog { get; set; }
    //    public string DeviceId { get; set; }
    //    public string Direction { get; set; } // up/down
    //    public string Function { get; set; }

    //}

    public enum DevicePermission : byte
    {
        Read = 1,   // 只能看数据
        Control = 2,   // 下发命令
        Manage = 3    // 参数、校准、绑定
    }

    [Table("Device")]
    public class DeviceEntity
    {
        [Key]
        [MaxLength(64)]
        public string DeviceId { get; set; }           // 主键

        [Required]
        [MaxLength(200)]
        public string DeviceName { get; set; }

        public DateTime? LastOnlineTime { get; set; } // 上次在线时间

        public bool? Status { get; set; } // 在线状态，true = 在线
    }

    [Table("User")]
    public class UserEntity
    {
        [Key]
        public long UserId { get; set; }

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

    [Table("UserDevice")]
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

    [Table("DeviceData")]
    public class DeviceDataEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }                               // 数据库自增主键

        [Required]
        [MaxLength(50)]
        public string Owner { get; set; } = string.Empty;           // 设备所属者

        [Required]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;        // 设备唯一ID

        [Required]
        [MaxLength(50)]
        public string Catalog { get; set; } = string.Empty;         // 设备分类

        [Required]
        [MaxLength(50)]
        public string Function { get; set; } = string.Empty;        // 功能类型：data/status/alert

        [Required]
        public string Payload { get; set; } = string.Empty;         // 原始JSON数据

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;   // 上报时间
    }

    [Table("CnemcData")]
    public class CnemcDataEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }                               // 数据库自增主键

        [Required]
        [MaxLength(50)]
        public string Owner { get; set; } = string.Empty;           // 设备所属者

        [Required]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;        // 设备唯一ID

        [Required]
        [MaxLength(50)]
        public string Catalog { get; set; } = string.Empty;         // 设备分类

        [Required]
        [MaxLength(50)]
        public string Function { get; set; } = string.Empty;        // 功能类型：data/status/alert

        [Required]
        public string Payload { get; set; } = string.Empty;         // 原始JSON数据

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;   // 上报时间
    }

    [Table("MqttMessageQueue")]
    public class MqttMessageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Topic { get; set; }

        [Required]
        public string Payload { get; set; }

        [Required]
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool Forwarded { get; set; } = false;
    }

    [Table("DeviceCommand")]
    public class DeviceCommandEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Owner { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Catalog { get; set; } = string.Empty;

        [Required]
        public string Function { get; set; } = string.Empty;

        [Required]
        public string Payload { get; set; } = string.Empty;

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow; //指令收到事件

        [Required]
        public bool Acked { get; set; } = false;                  // 是否收到设备回执

        public DateTime? AckTime { get; set; } // 回执时间
    }
}
