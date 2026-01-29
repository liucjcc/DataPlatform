using System;
using System.Collections.Generic;

namespace PlatformManager.DataModels
{
    /// <summary>
    /// 用户 DTO
    /// </summary>
 public class LoginDto
 {
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Role { get; set; } = "user"; // admin / user / readonly
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 设备 DTO
    /// </summary>
    public class DeviceDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户设备权限 DTO
    /// </summary>
    public class UserDevicePermissionDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public DevicePermission Permission { get; set; } = DevicePermission.Read;
    }

    /// <summary>
    /// 更新用户设备 DTO（差量更新用，通过 Username）
    /// </summary>
    public class UpdateUserDevicesDto
    {
        public string Username { get; set; } = string.Empty;
        public List<UserDevicePermissionDto> Devices { get; set; } = new List<UserDevicePermissionDto>();
    }

    /// <summary>
    /// 设备权限枚举
    /// </summary>
    public enum DevicePermission : byte
    {
        Read = 1,    // 只能看数据
        Control = 2, // 下发命令
        Manage = 3   // 参数、校准、绑定
    }
}
