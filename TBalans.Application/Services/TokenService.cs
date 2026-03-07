using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

public class TokenService : ITokenService
{
    // Gerçek bir sistemde bu IConfiguration üzerinden (appsettings.json) alınmalıdır. 
    // Kullanıcının "/first" workflow'unda verdiği örnek Program.cs ayarlarıyla uyuşması için buraya eklendi.
    private const string SecretKey = "TBalans_Super_Secret_Key_For_Jwt_Auth_2026!";
    private const string Issuer = "TBalansApp";
    private const string Audience = "TBalansUsers";

    public Task<string> GenerateJwtTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("university", user.University ?? string.Empty),
            new Claim("department", user.Department ?? string.Empty),
            new Claim("avatarType", user.AvatarType.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // Token süresi (7 gün)
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }
}
