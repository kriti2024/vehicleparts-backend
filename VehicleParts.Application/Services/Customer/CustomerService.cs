using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Vehicle;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services.Customer;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _context;

    public CustomerService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO dto)
    {
        var customer = new Domain.Entities.Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

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
        return await _context.Vehicles
            .Include(v => v.Customer)
            .Where(v =>
                v.Customer != null &&
                (
                    v.Customer.FullName.Contains(keyword) ||
                    v.Customer.Phone.Contains(keyword) ||
                    (v.Customer.Email != null && v.Customer.Email.Contains(keyword)) ||
                    v.VehicleNumber.Contains(keyword) ||
                    v.Model.Contains(keyword)
                ))
            .Select(v => new CustomerSearchDTO
            {
                CustomerId = v.Customer!.CustomerId,
                FullName = v.Customer.FullName,
                Phone = v.Customer.Phone,
                Email = v.Customer.Email,
                VehicleNumber = v.VehicleNumber,
                Model = v.Model
            })
            .ToListAsync();
    }
}