using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class EsewaPayment
{
    [Key]
    public int EsewaPaymentId { get; set; }

    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    [MaxLength(80)]
    public string TransactionUuid { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? TransactionCode { get; set; }

    [MaxLength(40)]
    public string ProductCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ProductServiceCharge { get; set; }
    public decimal ProductDeliveryCharge { get; set; }
    public decimal TotalAmount { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "Initiated";

    [MaxLength(512)]
    public string? Signature { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
}
