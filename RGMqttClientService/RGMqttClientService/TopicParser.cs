
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGMqttClientService
{
    public static class TopicParser
    {
        public static bool TryParse(
            string topic,
            string root,
            out MqttTopicInfo info)
        {
            info = null;

            if (string.IsNullOrWhiteSpace(topic))
                return false;

            var parts = topic.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

            // root + 5 段
            if (parts.Length != 6)
                return false;

            if (!string.Equals(parts[0], root, StringComparison.OrdinalIgnoreCase))
                return false;

            info = new MqttTopicInfo
            {
                Owner = parts[1],
                Catalog = parts[2],
                DeviceId = parts[3],
                Direction = parts[4],
                Function = parts[5]
            };

            return true;
        }
    }

}
