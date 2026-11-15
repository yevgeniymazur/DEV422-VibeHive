// Yevgeniy Mazur
// JWT token creation.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VibeHive.Contracts;
namespace VibeHive.Api.Events;
public sealed class TokenService(SymmetricSecurityKey key)
{
    public AuthResponse Issue(UserDto user)
    {
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(issuer: "VibeHive", audience: "VibeHive.Desktop",
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Role, user.Role), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) },
            expires: expires.UtcDateTime, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), expires, user);
    }
}
