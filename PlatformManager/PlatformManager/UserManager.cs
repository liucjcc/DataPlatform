
using PlatformManager.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserManager
{
    private readonly ApiService _apiService;

    public UserManager(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    #region 用户 CRUD
    public async Task<List<UserDto>> GetAllUsersAsync()
        => await _apiService.GetUsersAsync();

    public async Task<bool> AddUserAsync(UserDto user)
        => await _apiService.AddUserAsync(user);

    public async Task<bool> UpdateUserAsync(string username, UserDto user)
        => await _apiService.UpdateUserAsync(username, user);

    public async Task<bool> DeleteUserAsync(string username)
        => await _apiService.DeleteUserAsync(username);
    #endregion

    #region 用户设备
    public async Task<List<UserDevicePermissionDto>> GetUserDevicesAsync(string username)
        => await _apiService.GetUserDevicesAsync(username);

    public async Task<bool> UpdateUserDevicesAsync(string username, List<UserDevicePermissionDto> devices)
    {
        var dto = new UpdateUserDevicesDto
        {
            Username = username,
            Devices = devices
        };
        return await _apiService.UpdateUserDevicesAsync(dto);
    }

    public async Task<bool> AddDevicesToUserAsync(string username, List<string> deviceIds, DevicePermission permission = DevicePermission.Read)
    {
        var currentDevices = await _apiService.GetUserDevicesAsync(username);
        var currentIds = currentDevices.Select(d => d.DeviceId).ToHashSet();

        var newDevices = deviceIds
            .Where(id => !currentIds.Contains(id))
            .Select(id => new UserDevicePermissionDto { DeviceId = id, Permission = permission })
            .ToList();

        if (!newDevices.Any()) return true;

        var finalDevices = currentDevices.Concat(newDevices).ToList();
        return await UpdateUserDevicesAsync(username, finalDevices);
    }

    public async Task<bool> RemoveDevicesFromUserAsync(string username, List<string> deviceIds)
    {
        var currentDevices = await _apiService.GetUserDevicesAsync(username);
        var finalDevices = currentDevices
            .Where(d => !deviceIds.Contains(d.DeviceId))
            .ToList();

        return await UpdateUserDevicesAsync(username, finalDevices);
    }
    #endregion
}
