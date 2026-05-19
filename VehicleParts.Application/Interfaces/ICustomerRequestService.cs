using VehicleParts.Application.DTOs.CustomerRequests;

namespace VehicleParts.Application.Interfaces;

public interface ICustomerRequestService
{
    Task<PartRequestDto> CreateRequestAsync(CreatePartRequestDto dto);
    Task<List<PartRequestDto>> GetCustomerRequestsAsync(int customerId);
    Task<List<PartRequestDto>> GetAllRequestsAsync();
    Task<PartRequestDto?> UpdateRequestStatusAsync(int requestId, string status);
}
