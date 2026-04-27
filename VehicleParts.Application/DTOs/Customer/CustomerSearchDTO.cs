namespace VehicleParts.Application.DTOs.Customer;

public class CustomerSearchDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}