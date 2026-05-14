using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class PurchaseInvoiceItem
{
    [Key]
    public int PurchaseInvoiceItemId { get; set; }

    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public int PartId { get; set; }
    public Part? Part { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
