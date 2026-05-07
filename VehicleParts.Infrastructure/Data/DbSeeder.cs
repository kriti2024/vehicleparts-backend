using Microsoft.AspNetCore.Identity;
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

        await EnsureUserAsync(
            userManager,
            "admin@vehicleparts.com",
            "Admin User",
            "Admin#12345",
            Roles.Admin);

        await EnsureUserAsync(
            userManager,
            "staff@vehicleparts.com",
            "Default Staff",
            "Staff#12345",
            Roles.Staff);

        await EnsureUserAsync(
            userManager,
            "customer@vehicleparts.com",
            "Default Customer",
            "Customer#12345",
            Roles.Customer);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var existing =
            await userManager.FindByEmailAsync(email);

        if (existing is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            DateOfBirth = new DateTime(
                2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
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