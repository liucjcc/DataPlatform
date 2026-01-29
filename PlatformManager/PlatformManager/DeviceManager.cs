
using PlatformManager.DataModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DeviceManager
{
    private readonly ApiService _apiService;

    public DeviceManager(ApiService apiService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    /// <summary>
    /// 获取所有设备
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
        => await _apiService.GetAllDevicesAsync();

    /// <summary>
    /// 新增设备
    /// </summary>
    public async Task<bool> AddDeviceAsync(DeviceDto device)
        => await _apiService.AddDeviceAsync(device);

    /// <summary>
    /// 删除设备
    /// </summary>
    public async Task<bool> DeleteDeviceAsync(string deviceId)
        => await _apiService.DeleteDeviceAsync(deviceId);

    /// <summary>
    /// 修改设备名称
    /// </summary>
    public async Task<bool> UpdateDeviceAsync(string deviceId, string newDeviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("deviceId 不能为空", nameof(deviceId));
        if (string.IsNullOrWhiteSpace(newDeviceName))
            throw new ArgumentException("newDeviceName 不能为空", nameof(newDeviceName));

        var dto = new DeviceDto
        {
            DeviceId = deviceId,
            DeviceName = newDeviceName
        };

        return await _apiService.UpdateDeviceAsync(dto);
    }
}
