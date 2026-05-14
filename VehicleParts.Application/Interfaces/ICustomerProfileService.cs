using VehicleParts.Application.DTOs.CustomerProfile;

namespace VehicleParts.Application.Interfaces;

public interface ICustomerProfileService
{
    Task<CustomerProfileDetailsDto?> GetProfileAsync(int customerId);
    Task<CustomerProfileDetailsDto?> UpdateProfileAsync(int customerId, UpdateCustomerProfileDto dto);
}
