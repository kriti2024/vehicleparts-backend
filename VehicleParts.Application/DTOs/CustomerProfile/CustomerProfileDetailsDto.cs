namespace VehicleParts.Application.DTOs.CustomerProfile;

public class CustomerProfileDetailsDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal CreditBalance { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public bool CreditIsOverdue { get; set; }
    public decimal TotalSpend { get; set; }
    public List<CustomerVehicleDto> Vehicles { get; set; } = [];
}

public class CustomerVehicleDto
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
