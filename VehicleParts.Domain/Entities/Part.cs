using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Part
{
    [Key]
    public int PartId { get; set; }

    [Required]
    [MaxLength(150)]
    public string PartName { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    public int VendorId { get; set; }

    public Vendor? Vendor { get; set; }
}