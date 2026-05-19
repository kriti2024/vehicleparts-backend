namespace VehicleParts.Application.DTOs.CustomerRequests;

public class PartRequestDto
{
    public int PartRequestId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string? VehicleModel { get; set; }
    public string? Details { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
