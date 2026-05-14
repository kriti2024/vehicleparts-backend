using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehicleParts.Domain.Constants;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var configuration =
            services.GetRequiredService<IConfiguration>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
            }
        }

        await EnsureConfiguredUserAsync(
            userManager,
            configuration,
            "Admin",
            "Admin");

        await EnsureConfiguredUserAsync(
            userManager,
            configuration,
            "Staff",
            "Staff");
    }

    private static async Task EnsureConfiguredUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        string sectionName,
        string role)
    {
        var email = configuration[$"SeedUsers:{sectionName}:Email"];
        var fullName = configuration[$"SeedUsers:{sectionName}:FullName"]
            ?? sectionName;
        var password = configuration[$"SeedUsers:{sectionName}:Password"];

        if (string.IsNullOrWhiteSpace(email))
            return;

        var existing = await userManager.FindByEmailAsync(email);

        if (existing != null)
        {
            existing.FullName = fullName;
            existing.EmailConfirmed = true;
            existing.IsActive = true;

            await userManager.UpdateAsync(existing);

            if (!await userManager.IsInRoleAsync(existing, role))
            {
                await userManager.AddToRoleAsync(existing, role);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(password))
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result =
            await userManager.CreateAsync(
                user,
                password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(
                user,
                role);
        }
    }
}
