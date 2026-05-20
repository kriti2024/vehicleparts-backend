using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Vendor
{
    [Key]
    public int VendorId { get; set; }

    [Required]
    [MaxLength(150)]
    public string VendorName { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}