using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.CustomerRequests;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Services.Customer;

public class CustomerRequestService(IApplicationDbContext context) : ICustomerRequestService
{
    public async Task<PartRequestDto> CreateRequestAsync(CreatePartRequestDto dto)
    {
        var customerExists = await context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
        if (!customerExists)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        var request = new PartRequest
        {
            CustomerId = dto.CustomerId,
            PartName = dto.PartName,
            VehicleModel = dto.VehicleModel,
            Details = dto.Details
        };

        context.PartRequests.Add(request);
        await context.SaveChangesAsync();

        var customer = await context.Customers.FindAsync(dto.CustomerId);

        return new PartRequestDto
        {
            PartRequestId = request.PartRequestId,
            CustomerId = request.CustomerId,
            CustomerName = customer?.FullName ?? "",
            PartName = request.PartName,
            VehicleModel = request.VehicleModel,
            Details = request.Details,
            Status = request.Status,
            RequestedAt = request.RequestedAt
        };
    }

    public async Task<List<PartRequestDto>> GetCustomerRequestsAsync(int customerId)
    {
        return await context.PartRequests
            .Include(x => x.Customer)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new PartRequestDto
            {
                PartRequestId = x.PartRequestId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null ? x.Customer.FullName : "",
                PartName = x.PartName,
                VehicleModel = x.VehicleModel,
                Details = x.Details,
                Status = x.Status,
                RequestedAt = x.RequestedAt
            })
            .ToListAsync();
    }

    public async Task<List<PartRequestDto>> GetAllRequestsAsync()
    {
        return await context.PartRequests
            .Include(x => x.Customer)
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new PartRequestDto
            {
                PartRequestId = x.PartRequestId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null ? x.Customer.FullName : "",
                PartName = x.PartName,
                VehicleModel = x.VehicleModel,
                Details = x.Details,
                Status = x.Status,
                RequestedAt = x.RequestedAt
            })
            .ToListAsync();
    }

    public async Task<PartRequestDto?> UpdateRequestStatusAsync(int requestId, string status)
    {
        var request = await context.PartRequests
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.PartRequestId == requestId);

        if (request == null) return null;

        request.Status = status;
        await context.SaveChangesAsync();

        return new PartRequestDto
        {
            PartRequestId = request.PartRequestId,
            CustomerId = request.CustomerId,
            CustomerName = request.Customer != null ? request.Customer.FullName : "",
            PartName = request.PartName,
            VehicleModel = request.VehicleModel,
            Details = request.Details,
            Status = request.Status,
            RequestedAt = request.RequestedAt
        };
    }
}
