using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGMqttClientService
{
    public class MqttTopicInfo
    {
        public string Owner { get; set; }
        public string Catalog { get; set; }
        public string DeviceId { get; set; }
        public string Direction { get; set; } // up/down
        public string Function { get; set; }
    }
    public class DeviceCommand
    {
        public string CommandId { get; set; }      // 唯一命令ID
        public string Owner { get; set; }
        public string Catalog { get; set; }
        public string DeviceId { get; set; }       // 命令目标设备
        public string Function { get; set; }       // 命令类型，例如 "reboot", "setParam"
        public string Payload { get; set; }        // 命令内容（JSON 或文本）
        public DateTime Timestamp { get; set; }    // 下发时间
    }

    public class CommandAck
    {
        public string CommandId { get; set; }      // 对应服务端命令的 CommandId
        public string DeviceId { get; set; }       // 回执来源设备
        public string Status { get; set; }         // 执行状态，例如 "Success", "Failed"
        public string Result { get; set; }         // 可选结果信息，例如错误原因或返回值
        public long Timestamp { get; set; }        // 设备执行完成时间（Unix 秒或毫秒）
    }
}