using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.CustomerBooking;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services.Customer;

public class CustomerBookingService(IApplicationDbContext context) : ICustomerBookingService
{
    public async Task<ServiceBookingDto> CreateBookingAsync(CreateServiceBookingDto dto)
    {
        var customerExists = await context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
        if (!customerExists)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        var booking = new ServiceBooking
        {
            CustomerId = dto.CustomerId,
            VehicleNumber = dto.VehicleNumber,
            AppointmentDate = dto.AppointmentDate.ToUniversalTime(),
            Notes = dto.Notes
        };

        context.ServiceBookings.Add(booking);
        await context.SaveChangesAsync();

        return new ServiceBookingDto
        {
            ServiceBookingId = booking.ServiceBookingId,
            CustomerId = booking.CustomerId,
            VehicleNumber = booking.VehicleNumber,
            AppointmentDate = booking.AppointmentDate,
            Notes = booking.Notes,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };
    }

    public async Task<List<ServiceBookingDto>> GetCustomerBookingsAsync(int customerId)
    {
        return await context.ServiceBookings
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.AppointmentDate)
            .Select(x => new ServiceBookingDto
            {
                ServiceBookingId = x.ServiceBookingId,
                CustomerId = x.CustomerId,
                VehicleNumber = x.VehicleNumber,
                AppointmentDate = x.AppointmentDate,
                Notes = x.Notes,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }
}
