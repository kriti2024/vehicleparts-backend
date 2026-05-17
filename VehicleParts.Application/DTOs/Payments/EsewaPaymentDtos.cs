namespace VehicleParts.Application.DTOs.Payments;

public class EsewaPaymentRequestDto
{
    public decimal? Amount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ProductServiceCharge { get; set; }
    public decimal? ProductDeliveryCharge { get; set; }
}

public class EsewaPaymentInitiationDto
{
    public int SaleId { get; set; }
    public string FormAction { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Fields { get; set; } = new();
}

public class EsewaPaymentVerificationDto
{
    public bool Success { get; set; }
    public int? SaleId { get; set; }
    public string TransactionUuid { get; set; } = string.Empty;
    public string? TransactionCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
