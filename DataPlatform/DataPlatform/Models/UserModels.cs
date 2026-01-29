using DataPlatform.Shared.Entities;

namespace DataPlatform.Models
{
    public class UserDto
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string? DisplayName { get; set; }
        public string Role { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UpdateUserDevicesDto
    {
        public long UserId { get; set; }
        public List<UserDevicePermissionDto> Devices { get; set; } = [];
    }

    public class UserDevicePermissionDto
    {
        public string DeviceId { get; set; }
        public DevicePermission Permission { get; set; }
    }
}
