using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class PurchaseInvoice
{
    [Key]
    public int PurchaseInvoiceId { get; set; }

    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}
