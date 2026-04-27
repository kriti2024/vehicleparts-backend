using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Vehicle;

namespace VehicleParts.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO dto);
    Task<CustomerDTO?> GetCustomerByIdAsync(int customerId);
    Task<CustomerWithVehiclesDTO?> GetCustomerWithVehiclesAsync(int customerId);
    Task<List<CustomerDTO>> GetAllCustomersAsync();
    Task<VehicleDTO> AddVehicleAsync(CreateVehicleDTO dto);
    Task<List<VehicleDTO>> GetCustomerVehiclesAsync(int customerId);
}