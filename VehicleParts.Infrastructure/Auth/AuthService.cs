using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.Exceptions;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Constants;
using VehicleParts.Domain.Entities;
using VehicleParts.Infrastructure.Data;

namespace VehicleParts.Infrastructure.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    AppDbContext context)
    : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing =
            await userManager.FindByEmailAsync(dto.Email);

        if (existing != null)
            throw new BadRequestException(
                "Email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result =
            await userManager.CreateAsync(
                user,
                dto.Password);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ",
                result.Errors.Select(x => x.Description)));

        await userManager.AddToRoleAsync(
            user,
            Roles.Customer);

        var customer = new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        };

        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        // Create vehicle
        var vehicle = new Vehicle
        {
            CustomerId = customer.CustomerId,
            VehicleNumber = dto.VehicleNumber,
            Model = dto.VehicleModel,
            Brand = dto.VehicleBrand,
            Year = dto.VehicleYear
        };

        context.Vehicles.Add(vehicle);

        await context.SaveChangesAsync();

        return await BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user =
            await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
            throw new UnauthorizedException(
                "Invalid email or password.");

        var valid =
            await userManager.CheckPasswordAsync(
                user,
                dto.Password);

        if (!valid)
            throw new UnauthorizedException(
                "Invalid email or password.");

        return await BuildResponse(user);
    }

    private async Task<AuthResponseDto>
        BuildResponse(ApplicationUser user)
    {
        var roles =
            await userManager.GetRolesAsync(user);

        string? customerId = null;

        if (roles.Contains(Roles.Customer))
        {
            var customer =
                await context.Customers
                .FirstOrDefaultAsync(x =>
                    x.Email == user.Email);

            if (customer != null)
                customerId =
                    customer.CustomerId.ToString();
        }

        var (token, expiresAt) =
            tokenService.GenerateToken(
                user,
                roles,
                customerId);

        return new AuthResponseDto
        {
            UserId = user.Id.ToString(),
            Email = user.Email ?? "",
            FullName = user.FullName,
            Roles = roles,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

}
