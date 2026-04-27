using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class SaleItem
{
    [Key]
    public int SaleItemId { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int PartId { get; set; }
    public Part? Part { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => Quantity * UnitPrice;
}