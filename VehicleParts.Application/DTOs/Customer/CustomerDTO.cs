namespace VehicleParts.Application.DTOs.Customer;

public class CustomerDTO
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal CreditBalance { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public bool CreditIsOverdue { get; set; }
    public decimal TotalSpend { get; set; }
    public List<VehicleParts.Application.DTOs.Vehicle.VehicleDTO> Vehicles { get; set; } = new();
}
