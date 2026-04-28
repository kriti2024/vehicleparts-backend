using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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

        string[] roles = { "Admin", "Staff", "Customer" };

        foreach (var role in roles)
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
            "System Admin",
            "Admin@123",
            "Admin");

        await EnsureUserAsync(
            userManager,
            "staff@vehicleparts.com",
            "Default Staff",
            "Staff@123",
            "Staff");

        var context = services.GetRequiredService<AppDbContext>();
        await SeedVendorsAndPartsAsync(context);
    }

    private static async Task SeedVendorsAndPartsAsync(AppDbContext context)
    {
        if (context.Vendors.Any()) return;

        var vendor = new Vendor
        {
            VendorName = "Global Parts Corp",
            Phone = "123-456-7890",
            Address = "123 Main St, Industrial Zone"
        };
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        if (!context.Parts.Any())
        {
            context.Parts.AddRange(
                new Part { PartName = "Brake Pads", Price = 45.99m, StockQuantity = 15, VendorId = vendor.VendorId },
                new Part { PartName = "Oil Filter", Price = 12.50m, StockQuantity = 5, VendorId = vendor.VendorId },
                new Part { PartName = "Spark Plugs", Price = 8.00m, StockQuantity = 50, VendorId = vendor.VendorId },
                new Part { PartName = "Air Filter", Price = 18.25m, StockQuantity = 8, VendorId = vendor.VendorId }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(email);

        if (existing != null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}