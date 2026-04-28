using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Middlewares;
using VehicleParts.Infrastructure;
using VehicleParts.Infrastructure.Data;
using VehicleParts.Application.Interfaces;
using VehicleParts.Application.Services.Customer;
using VehicleParts.Application.Services.Sales;
using VehicleParts.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(
    builder.Configuration);

// Register Application Services (AFTER Infrastructure)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
builder.Services.AddScoped<ICustomerBookingService, CustomerBookingService>();
builder.Services.AddScoped<ICustomerRequestService, CustomerRequestService>();
builder.Services.AddScoped<ICustomerReviewService, CustomerReviewService>();
builder.Services.AddScoped<IStaffCustomerHistoryService, StaffCustomerHistoryService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();

// CORS for frontend
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Database + Seeder
using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(
        scope.ServiceProvider);
}

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();