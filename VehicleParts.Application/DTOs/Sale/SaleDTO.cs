namespace VehicleParts.Application.DTOs.Sale;

public class SaleDTO
{
    public int SaleId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }

    // Feature 16 - Loyalty Discount fields
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;
    public List<SaleItemDetailDTO> Items { get; set; } = new();
}

public class SaleItemDetailDTO
{
    public int SaleItemId { get; set; }
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}