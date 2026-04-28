using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class PurchaseInvoiceItem
{
    [Key]
    public int PurchaseInvoiceItemId { get; set; }

    [Required]
    public int PurchaseInvoiceId { get; set; }

    public PurchaseInvoice? PurchaseInvoice { get; set; }

    [Required]
    public int PartId { get; set; }

    public Part? Part { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => Quantity * UnitPrice;
}
