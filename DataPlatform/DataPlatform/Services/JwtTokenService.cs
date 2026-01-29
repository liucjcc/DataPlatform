
using DataPlatform.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataPlatform.Shared.Entities; 

public class JwtTokenService
{
    private readonly IConfiguration _config;

    // 模拟内存存储 RefreshToken
    private static readonly Dictionary<string, string> _refreshTokens = new();

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    // 生成 Access Token
    public (string token, DateTime expires) GenerateAccessToken(UserEntity user)
    {
        var claims = new[]
        {
            new Claim("uid", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expires);
    }

    // 生成 Refresh Token
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    // 存储 Refresh Token
    public void StoreRefreshToken(string refreshToken, string username)
    {
        _refreshTokens[refreshToken] = username;
    }

    // 刷新 Access Token
    public (string newAccessToken, DateTime expires) Refresh(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var username))
            throw new SecurityTokenException("Invalid refresh token");

        // 模拟用户信息
        var user = new UserEntity { Id = 1, Username = username, Role = "User" };
        return GenerateAccessToken(user);
    }
}

