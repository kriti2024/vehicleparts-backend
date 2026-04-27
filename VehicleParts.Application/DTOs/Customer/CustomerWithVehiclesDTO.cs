using VehicleParts.Application.DTOs.Vehicle;

namespace VehicleParts.Application.DTOs.Customer;

public class CustomerWithVehiclesDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<VehicleDTO> Vehicles { get; set; } = new();
}