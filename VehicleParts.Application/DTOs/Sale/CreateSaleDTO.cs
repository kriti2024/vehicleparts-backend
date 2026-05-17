using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Sale;

public class CreateSaleDTO
{
    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "Sale must have at least one item")]
    public List<SaleItemDTO> Items { get; set; } = new();

    public string? PaymentStatus { get; set; }
}

public class SaleItemDTO
{
    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
