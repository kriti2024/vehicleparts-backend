using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Brand { get; set; }

    public int? Year { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }
}