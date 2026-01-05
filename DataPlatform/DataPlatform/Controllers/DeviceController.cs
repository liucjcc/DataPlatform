using DataPlatform.Models;
using DataPlatform.Shared.DataAccess;
using DataPlatform.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataPlatform.Controllers
{
    [ApiController]
    [Route("api/device")]
    //[Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly DeviceRepository _deviceRepo;
        private static readonly Random _random = new Random();

        public DeviceController(ILogger<DeviceController> logger, DeviceRepository deviceRepo)
        {
            _logger = logger;
            _deviceRepo = deviceRepo ?? throw new ArgumentNullException(nameof(deviceRepo));
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllDevice()
        {
            try
            {
                var devices = _deviceRepo.GetAllDevices();
                return Ok(new
                {
                    success = true,
                    data = devices
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "获取设备列表失败",
                });
            }
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDeviceById(string deviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceId))
                    return BadRequest(new
                    {
                        success = false,
                        error = "未指定 DeviceId"
                    });

                var device = _deviceRepo.GetDeviceById(deviceId);
                if (device == null)
                    return NotFound(new
                    {
                        success = false,
                        error = $"DeviceId {deviceId} 不存在"
                    });

                return Ok(new
                {
                    success = true,
                    data = device
                });
            }
            catch (InvalidOperationException ex)
            {
                // 业务异常
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                // 系统异常，记录日志
                _logger.LogError(ex, "获取设备失败, DeviceId={DeviceId}", deviceId);

                return StatusCode(500, new
                {
                    success = false,
                    error = "获取设备信息失败"
                });
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddDevice([FromBody] DeviceConfigDto dto)
        {
            try
            {
                var entity = new DeviceConfigEntity
                {
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    LastOnlineTime = DateTime.Now,
                    Status = false
                };

                _deviceRepo.AddDevice(entity);

                return Ok(new
                {
                    success = true,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加设备失败, DTO ={ @DTO}", dto);
                return StatusCode(500, new
                {
                    success = false,
                    error = "添加设备失败",
                });
            }
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateDevice([FromBody] DeviceConfigDto dto)
        {
            try
            {
                var entity = new DeviceConfigEntity
                {
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    LastOnlineTime = DateTime.Now,
                    Status = false
                };

                _deviceRepo.UpdateDevice(entity);

                return Ok(new
                {
                    success = true,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备失败, DTO ={ @DTO}", dto);
                return StatusCode(500, new
                {
                    success = false,
                    error = "更新设备失败",
                });
            }
        }

        [HttpDelete("{deviceId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteDevice(string deviceId)
        {
            try
            {
                var deletedDevice = _deviceRepo.DeleteDevice(deviceId);
                return Ok(new
                {
                    success = true,
                    data = deletedDevice
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备失败, DeviceId={DeviceId}", deviceId);
                return StatusCode(500, new
                {
                    success = false,
                    error = "删除设备失败"
                });
            }
        }

        [HttpGet("{deviceId}/latest")]
        [AllowAnonymous]
        public IActionResult GetLatestData(string deviceId)
        {
            try
            {
                var data = _deviceRepo.GetLatestData(deviceId);
                return Ok(new { success = true, data });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        // 实时数据（HTTP 轮询版）
        [HttpGet("{deviceId}/realtimeData")]
        public IActionResult GetRealtimeData(string deviceId)
        {
            var data = new WeatherRealtimeDto
            {
                DeviceId = deviceId,
                DeviceName = "延安一路站点-RG4702",
                Timestamp = DateTime.UtcNow,
                Temperature = RandomRange(15, 30),
                Humidity = RandomRange(40, 80),
                Pressure = RandomRange(990, 1030),
                WindSpeed = RandomRange(0, 10)
            };

            return Ok(data);
        }

        // 历史数据
        [HttpGet("{deviceId}/historyData")]
        public IActionResult GetHistoryData(
            string deviceId,
            [FromQuery] int hours = 24)
        {
            var list = new List<WeatherHistoryDto>();

            for (int i = hours; i > 0; i--)
            {
                list.Add(new WeatherHistoryDto
                {
                    Timestamp = DateTime.UtcNow.AddHours(-i),
                    Temperature = RandomRange(14, 28),
                    Humidity = RandomRange(45, 75)
                });
            }

            return Ok(new
            {
                deviceId,
                points = list
            });
        }

        private static double RandomRange(double min, double max)
            => Math.Round(min + _random.NextDouble() * (max - min), 2);
    }
}

