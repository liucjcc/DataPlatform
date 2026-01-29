using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPlatform.Shared.Entities
{
    public class DeviceEntity
    {
        [Key]
        public string DeviceId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DeviceName { get; set; }

        public DateTime? LastOnlineTime { get; set; } 

        public bool? Status { get; set; }
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
        [MaxLength(50)]
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
}
