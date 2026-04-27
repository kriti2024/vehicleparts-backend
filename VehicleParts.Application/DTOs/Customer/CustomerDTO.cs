namespace VehicleParts.Application.DTOs.Customer;

public class CustomerDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}