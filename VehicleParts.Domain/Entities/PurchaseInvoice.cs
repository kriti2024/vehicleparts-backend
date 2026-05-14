using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class PurchaseInvoice
{
    [Key]
    public int PurchaseInvoiceId { get; set; }

    [Required]
    public int VendorId { get; set; }

    public Vendor? Vendor { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    // Navigation property - One PurchaseInvoice has many PurchaseInvoiceItems
    public ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();
}
