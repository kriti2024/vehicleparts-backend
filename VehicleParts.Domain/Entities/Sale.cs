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

    public decimal TotalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
}