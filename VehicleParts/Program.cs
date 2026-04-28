using Microsoft.EntityFrameworkCore;
using VehicleParts.Infrastructure;
using VehicleParts.Infrastructure.Data;
using VehicleParts.Application.Interfaces;
using VehicleParts.Application.Services.Customer;
using VehicleParts.Application.Services.Sales;
using VehicleParts.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Register Application Services (AFTER Infrastructure)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();