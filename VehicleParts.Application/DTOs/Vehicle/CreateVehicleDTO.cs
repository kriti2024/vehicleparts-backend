using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Vehicle;

public class CreateVehicleDTO
{
    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Vehicle number is required")]
    [StringLength(50, ErrorMessage = "Vehicle number cannot exceed 50 characters")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    public string Model { get; set; } = string.Empty;
}