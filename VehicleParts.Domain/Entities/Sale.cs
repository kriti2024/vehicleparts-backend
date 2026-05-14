using System.ComponentModel.DataAnnotations;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Domain.Entities;

public class Sale
{
    [Key]
    public int SaleId { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    // For Feature 16 - Loyalty Discount System
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

    // Navigation property - One Sale has many SaleItems
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<EsewaPayment> EsewaPayments { get; set; } = new List<EsewaPayment>();
}
