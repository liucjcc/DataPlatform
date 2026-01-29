using PlatformManager.DataModels;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ApiService
{
    private readonly HttpClient _client;
    public void SetToken(string token)
    {
        if (_client.DefaultRequestHeaders.Contains("Authorization"))
            _client.DefaultRequestHeaders.Remove("Authorization");

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
    public async Task<LoginResponseDto> LoginAsync(string username, string password)
    {
        var dto = new LoginDto
        {
            Username = username,
            Password = password
        };

        try
        {
            var response = await _client.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
                return null; // 登录失败

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResult;
        }
        catch
        {
            return null;
        }
        ;
    }

    public ApiService(string baseUrl, string token)
    {
        _client = new HttpClient { BaseAddress = new System.Uri(baseUrl) };
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    #region 用户接口
    public async Task<List<UserDto>> GetUsersAsync()
        => await _client.GetFromJsonAsync<List<UserDto>>("api/user/list") ?? new List<UserDto>();

    public async Task<bool> AddUserAsync(UserDto user)
        => (await _client.PostAsJsonAsync("api/user/add", user)).IsSuccessStatusCode;

    public async Task<bool> UpdateUserAsync(string username, UserDto user)
        => (await _client.PutAsJsonAsync($"api/user/{username}", user)).IsSuccessStatusCode;

    public async Task<bool> DeleteUserAsync(string username)
        => (await _client.DeleteAsync($"api/user/{username}")).IsSuccessStatusCode;

    public async Task<List<UserDevicePermissionDto>> GetUserDevicesAsync(string username)
        => await _client.GetFromJsonAsync<List<UserDevicePermissionDto>>($"api/user/{username}/devices")
           ?? new List<UserDevicePermissionDto>();

    public async Task<bool> UpdateUserDevicesAsync(UpdateUserDevicesDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var res = await _client.PutAsync("api/user/devices",
            new StringContent(json, Encoding.UTF8, "application/json"));
        return res.IsSuccessStatusCode;
    }
    #endregion

    #region 设备接口
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
        => await _client.GetFromJsonAsync<List<DeviceDto>>("api/device/list") ?? new List<DeviceDto>();

    public async Task<bool> AddDeviceAsync(DeviceDto device)
        => (await _client.PostAsJsonAsync("api/device/add", device)).IsSuccessStatusCode;

    public async Task<bool> DeleteDeviceAsync(string deviceId)
        => (await _client.DeleteAsync($"api/device/{deviceId}")).IsSuccessStatusCode;
    public async Task<bool> UpdateDeviceAsync(DeviceDto device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));
        if (string.IsNullOrWhiteSpace(device.DeviceId))
            throw new ArgumentException("DeviceId 不能为空", nameof(device.DeviceId));

        var json = JsonSerializer.Serialize(device);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // PUT /api/device/{deviceId}，后端根据 DeviceId 更新对应设备
        var response = await _client.PutAsync($"api/device/{device.DeviceId}", content);
        return response.IsSuccessStatusCode;
    }
    #endregion
}
