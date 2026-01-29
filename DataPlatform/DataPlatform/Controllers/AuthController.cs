using DataPlatform.Models;
using DataPlatform.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataPlatform.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwt;

        public AuthController(JwtTokenService jwt)
        {
            _jwt = jwt;
        }

        public static class RefreshTokenStore
        {
            // 模拟：已失效的 refresh token
            public static HashSet<string> RevokedTokens { get; } = new();
        }

        [HttpPost("login")]

        public IActionResult Login([FromBody] LoginDto dto)
        {
            UserEntity user;

            // 模拟两个账号
            if (dto.Username == "admin" && dto.Password == "12345")
            {
                user = new UserEntity
                {
                    Id = 1,
                    Username = "admin",
                    Role = "Admin"
                };
            }
            else if (dto.Username == "user" && dto.Password == "12345")
            {
                user = new UserEntity
                {
                    Id = 2,
                    Username = "user",
                    Role = "User"
                };
            }
            else
            {
                return Unauthorized();
            }

            // 生成 Access Token
            var (accessToken, expires) = _jwt.GenerateAccessToken(user);

            // 生成 Refresh Token
            var refreshToken = _jwt.GenerateRefreshToken();
            _jwt.StoreRefreshToken(refreshToken, user.Username);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expires,
                Role = user.Role   // 调试时很有用
            });
        }

        // Refresh 接口
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshDto req)
        {
            Console.WriteLine($"收到 RefreshToken: {req.RefreshToken}");
            try
            {
                var (newToken, expires) = _jwt.Refresh(req.RefreshToken);
                return Ok(new { AccessToken = newToken, ExpiresAt = expires });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout([FromBody] LogoutDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                return BadRequest(new
                {
                    success = false,
                    erro = "refreshToken is required"
                });
            }

            RefreshTokenStore.RevokedTokens.Add(dto.RefreshToken);

            return Ok(new
            {
                success = true,
                data = dto
            }); 
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;

            var roles = User
                .FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList();

            var result = new 
            {
                success = true,
                data = new
                {
                    userId,
                    username,
                    roles
                }
            };

            return Ok(result);
        }
    }
}





