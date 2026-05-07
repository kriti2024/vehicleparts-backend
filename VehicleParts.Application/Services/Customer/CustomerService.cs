using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Vehicle;
using VehicleParts.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using VehicleParts.Domain.Constants;
using VehicleParts.Domain.Entities;
using VehicleParts.Application.Interfaces.Email;

namespace VehicleParts.Application.Services.Customer;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService? _emailService;

    public CustomerService(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailService? emailService = null)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO dto)
    {
        // Check existing customer
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == dto.Email);

        if (existingCustomer != null)
            throw new Exception("Customer email already exists");

        // Create customer record
        var customer = new Domain.Entities.Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Generate temporary password
        var tempPassword = "Cust@" + Random.Shared.Next(1000, 9999);

        // Create login account
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, tempPassword);

        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ",
                result.Errors.Select(e => e.Description)));
        }

        // Assign customer role
        await _userManager.AddToRoleAsync(user, Roles.Customer);

        // Email (SAFE - only runs if available)
        if (_emailService != null)
        {
            var subject = "Vehicle Parts System - Customer Account Created";

            var body = $@"
Hello {dto.FullName},

Your customer account has been created successfully.

Login Credentials:

Email: {dto.Email}
Password: {tempPassword}

Please login and change your password.

Thank you,
Vehicle Parts Management System
";

            await _emailService.SendEmailAsync(dto.Email!, subject, body);
        }

        return new CustomerDTO
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email
        };
    }

    public async Task<CustomerDTO?> GetCustomerByIdAsync(int customerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null) return null;

        return new CustomerDTO
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email
        };
    }

    public async Task<CustomerWithVehiclesDTO?> GetCustomerWithVehiclesAsync(int customerId)
    {
        var customer = await _context.Customers
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null) return null;

        return new CustomerWithVehiclesDTO
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Vehicles = customer.Vehicles.Select(v => new VehicleDTO
            {
                VehicleId = v.VehicleId,
                VehicleNumber = v.VehicleNumber,
                Model = v.Model,
                CustomerId = v.CustomerId,
                CustomerName = customer.FullName
            }).ToList()
        };
    }

    public async Task<List<CustomerDTO>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Select(c => new CustomerDTO
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email
            })
            .ToListAsync();
    }

    public async Task<VehicleDTO> AddVehicleAsync(CreateVehicleDTO dto)
    {
        var customerExists = await _context.Customers
            .AnyAsync(c => c.CustomerId == dto.CustomerId);

        if (!customerExists)
            throw new Exception($"Customer with ID {dto.CustomerId} not found");

        var vehicle = new Domain.Entities.Vehicle
        {
            VehicleNumber = dto.VehicleNumber,
            Model = dto.Model,
            CustomerId = dto.CustomerId
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var customer = await _context.Customers
            .FirstAsync(c => c.CustomerId == dto.CustomerId);

        return new VehicleDTO
        {
            VehicleId = vehicle.VehicleId,
            VehicleNumber = vehicle.VehicleNumber,
            Model = vehicle.Model,
            CustomerId = vehicle.CustomerId,
            CustomerName = customer.FullName
        };
    }

    public async Task<List<VehicleDTO>> GetCustomerVehiclesAsync(int customerId)
    {
        return await _context.Vehicles
            .Where(v => v.CustomerId == customerId)
            .Include(v => v.Customer)
            .Select(v => new VehicleDTO
            {
                VehicleId = v.VehicleId,
                VehicleNumber = v.VehicleNumber,
                Model = v.Model,
                CustomerId = v.CustomerId,
                CustomerName = v.Customer != null ? v.Customer.FullName : ""
            })
            .ToListAsync();
    }

    public async Task<List<CustomerSearchDTO>> SearchCustomersAsync(string keyword)
    {
        keyword = keyword.ToLower();

        var result = await _context.Customers
            .Include(c => c.Vehicles)
            .Where(c =>
                c.FullName.ToLower().Contains(keyword) ||
                c.Phone.Contains(keyword) ||
                c.CustomerId.ToString() == keyword ||
                c.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(keyword)))
            .SelectMany(c => c.Vehicles.DefaultIfEmpty(),
                (c, v) => new CustomerSearchDTO
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FullName,
                    Phone = c.Phone,
                    Email = c.Email,
                    VehicleNumber = v != null ? v.VehicleNumber : "",
                    Model = v != null ? v.Model : ""
                })
            .ToListAsync();

        return result;
    }
}