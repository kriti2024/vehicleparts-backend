using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Constants;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Infrastructure.Auth;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
    : IUserService
{
    public async Task<UserDto> CreateStaffAsync(CreateStaffDto dto)
    {
        if (!await roleManager.RoleExistsAsync(dto.Role))
            throw new Exception("Role does not exist.");

        var existing = await userManager.FindByEmailAsync(dto.Email);

        if (existing != null)
            throw new Exception("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ",
                result.Errors.Select(x => x.Description)));

        await userManager.AddToRoleAsync(user, dto.Role);

        return await MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllStaffAsync()
    {
        var users = await userManager.Users.ToListAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains(Roles.Staff) || roles.Contains(Roles.Admin))
            {
                result.Add(await MapToDto(user));
            }
        }

        return result;
    }

    public async Task DeleteStaffAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
            throw new Exception("User not found.");

        if (user.Email == "admin@vehicleparts.com")
            throw new Exception("Main admin cannot be disabled.");

        user.IsActive = false;
        await userManager.UpdateAsync(user);
    }

    public async Task ChangeRoleAsync(Guid id, string role)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
            throw new Exception("User not found.");

        if (!await roleManager.RoleExistsAsync(role))
            throw new Exception("Invalid role.");

        var roles = await userManager.GetRolesAsync(user);

        await userManager.RemoveFromRolesAsync(user, roles);

        await userManager.AddToRoleAsync(user, role);
    }

    private async Task<UserDto> MapToDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            IsActive = user.IsActive,
            Roles = await userManager.GetRolesAsync(user)
        };
    }
}