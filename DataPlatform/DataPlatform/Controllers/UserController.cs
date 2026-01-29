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
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserRepository _userRepo;
        private static readonly Random _random = new Random();

        public UserController(ILogger<UserController> logger, UserRepository userRepo)
        {
            _logger = logger;
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }
        [HttpGet("devices")]
        public IActionResult GetMyDevices()
        {
            var devices = new[]
            {
            new { id = "WS-1001", name = "采样器-市南" },
            new { id = "WS-1002", name = "采样器-市北" }
        };

            return Ok(devices);
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser([FromBody] UserDto dto)
        {
            try
            {
                if (_userRepo.UsernameExists(dto.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "用户名已存在"
                    });
                }

                var entity = new UserEntity
                {
                    Username = dto.Username,
                    PasswordHash = dto.PasswordHash,
                    DisplayName = dto.DisplayName,
                    Role = dto.Role,
                    IsEnabled = dto.IsEnabled
                };

                _userRepo.Add(entity);

                // 数据库生成的 Id 回写到 DTO（可选）
                var resultDto = new UserDto
                {
                    Username = entity.Username,
                    PasswordHash = entity.PasswordHash,
                    DisplayName = entity.DisplayName,
                    Role = entity.Role,
                    IsEnabled = entity.IsEnabled
                };

                return Ok(new
                {
                    success = true,
                    data = resultDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户失败, DTO={@DTO}", dto);
                return StatusCode(500, new
                {
                    success = false,
                    error = "添加用户失败"
                });
            }
        }


        [HttpPut("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UserDto dto)
        {
            try
            {
                var user = _userRepo.GetByUsername(username);
                if (user is null)
                    return NotFound(new { success = false, error = "用户不存在" });

                user.DisplayName = dto.DisplayName;
                user.Role = dto.Role;
                user.IsEnabled = dto.IsEnabled;

                if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                    user.PasswordHash = dto.PasswordHash;

                _userRepo.Update(user);

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户失败, DTO={@DTO}", dto);
                return StatusCode(500, new { success = false, error = "更新用户失败" });
            }
        }


        [HttpDelete("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var user = _userRepo.GetByUsername(username);
                if (user is null)
                    return NotFound(new { success = false, error = "用户不存在" });

                _userRepo.Delete(user.Id);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败, Username={Username}", username);
                return StatusCode(500, new { success = false, error = "删除用户失败" });
            }
        }


        [HttpPut("devices")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateUserDevices(
    [FromBody] UpdateUserDevicesDto dto)
        {
            try
            {
                var entities = dto.Devices.Select(d => new UserDeviceEntity
                {
                    DeviceId = d.DeviceId,
                    Permission = d.Permission
                }).ToList();

                _userRepo.UpdateUserDevicesDiff(dto.UserId, entities);

                return Ok(new
                {
                    success = true,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "差量更新用户设备权限失败, DTO={@DTO}", dto);

                return StatusCode(500, new
                {
                    success = false,
                    error = "更新用户设备权限失败"
                });
            }
        }




    }
}