namespace VehicleParts.Application.DTOs.Purchase;

public class PurchaseInvoiceDto
{
    public int PurchaseInvoiceId { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class PurchaseInvoiceItemDto
{
    public int PurchaseInvoiceItemId { get; set; }
    public int PartId { get; set; }
    public string? PartName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreatePurchaseInvoiceDto
{
    public int VendorId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class CreatePurchaseInvoiceItemDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
