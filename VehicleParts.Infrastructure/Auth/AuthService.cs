using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;


namespace VehicleParts.Infrastructure.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
            throw new Exception("Invalid email or password.");

        var validPassword =
            await userManager.CheckPasswordAsync(user, password);

        if (!validPassword)
            throw new Exception("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                configuration["JwtSettings:Key"]!));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        var tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResponseDto
        {
            UserId = user.Id.ToString(),
            Email = user.Email ?? email,
            FullName = user.FullName,
            Roles = roles.ToList(),
            Token = tokenValue
        };
    }
}
