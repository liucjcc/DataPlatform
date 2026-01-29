using System.ComponentModel.DataAnnotations;

namespace DataPlatform.Models
{
    public class WeatherRealtimeDto
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double WindSpeed { get; set; }
    }

    public class WeatherHistoryDto
    {
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }

    public class DeviceDataDto
    {
        public string DeviceId { get; set; }
        public WeatherRealtimeDto Realtime { get; set; }
        public List<WeatherHistoryDto> History { get; set; }
    }

    public class DeviceDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty; 
    }
}
