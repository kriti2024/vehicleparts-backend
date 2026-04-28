using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Infrastructure.Auth;

public class TokenService(
    IOptions<JwtSettings> options)
    : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAt)
        GenerateToken(
            ApplicationUser user,
            IEnumerable<string> roles,
            string? customerId = null)
    {
        var expiresAt =
            DateTime.UtcNow.AddMinutes(
                _settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(JwtRegisteredClaimNames.Email,
                user.Email ?? ""),

            new(ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(ClaimTypes.Email,
                user.Email ?? ""),

            new("fullName",
                user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        if (!string.IsNullOrWhiteSpace(customerId))
        {
            claims.Add(
                new Claim(
                    "CustomerId",
                    customerId));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _settings.Key));

        var creds =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

        return (
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAt);
    }
}