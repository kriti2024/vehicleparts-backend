using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.CustomerProfile;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services.Customer;

public class CustomerProfileService(IApplicationDbContext context) : ICustomerProfileService
{
    public async Task<CustomerProfileDetailsDto?> GetProfileAsync(int customerId)
    {
        return await context.Customers
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CustomerProfileDetailsDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                Vehicles = c.Vehicles
                    .Select(v => new CustomerVehicleDto
                    {
                        VehicleId = v.VehicleId,
                        VehicleNumber = v.VehicleNumber,
                        Model = v.Model
                    }).ToList(),
                TotalSpend = c.Sales.Sum(s => s.FinalAmount),
                CreditBalance = c.Sales.Where(s => s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Credit || s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Pending).Sum(s => s.FinalAmount),
                CreditDueDate = c.Sales.Where(s => s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Credit || s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Pending).OrderBy(s => s.SaleDate).Select(s => (DateTime?)s.SaleDate.AddMonths(1)).FirstOrDefault(),
                CreditIsOverdue = c.Sales.Any(s => (s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Credit || s.PaymentStatus == VehicleParts.Domain.Enums.PaymentStatus.Pending) && s.SaleDate <= DateTime.UtcNow.AddMonths(-1))
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerProfileDetailsDto?> UpdateProfileAsync(int customerId, UpdateCustomerProfileDto dto)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (customer == null)
        {
            return null;
        }

        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        await context.SaveChangesAsync();

        return await GetProfileAsync(customerId);
    }
}
