using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehicleParts.Application.DTOs.Payments;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Infrastructure.Services;

public class EsewaPaymentService : IEsewaPaymentService
{
    private const string SignedFieldNames = "total_amount,transaction_uuid,product_code";
    private readonly IApplicationDbContext _context;
    private readonly EsewaOptions _options;

    public EsewaPaymentService(
        IApplicationDbContext context,
        IOptions<EsewaOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<EsewaPaymentInitiationDto> InitiatePaymentAsync(
        int saleId,
        EsewaPaymentRequestDto request)
    {
        var sale = await _context.Sales
            .FirstOrDefaultAsync(s => s.SaleId == saleId);

        if (sale == null)
            throw new Exception($"Sale with ID {saleId} not found");

        if (sale.PaymentStatus == PaymentStatus.Paid)
            throw new Exception("This sale is already marked as paid.");

        var amount = sale.FinalAmount;
        var transactionUuid = $"SALE-{sale.SaleId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var initiation = await CreatePaymentAsync(
            amount,
            transactionUuid,
            request,
            sale.SaleId);

        sale.PaymentStatus = PaymentStatus.Pending;
        await _context.SaveChangesAsync();

        return initiation;
    }

    public async Task<EsewaPaymentInitiationDto> InitiateDirectPaymentAsync(
        EsewaPaymentRequestDto request)
    {
        if (!request.Amount.HasValue || request.Amount.Value <= 0)
            throw new Exception("Payment amount must be greater than zero.");

        return await CreatePaymentAsync(
            request.Amount.Value,
            $"DIRECT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            request,
            null);
    }

    private async Task<EsewaPaymentInitiationDto> CreatePaymentAsync(
        decimal amount,
        string transactionUuid,
        EsewaPaymentRequestDto request,
        int? saleId)
    {
        var taxAmount = request.TaxAmount ?? 0;
        var serviceCharge = request.ProductServiceCharge ?? 0;
        var deliveryCharge = request.ProductDeliveryCharge ?? 0;
        var totalAmount = amount + taxAmount + serviceCharge + deliveryCharge;
        var signature = GenerateRequestSignature(totalAmount, transactionUuid);

        var payment = new EsewaPayment
        {
            SaleId = saleId,
            TransactionUuid = transactionUuid,
            ProductCode = _options.ProductCode,
            Amount = amount,
            TaxAmount = taxAmount,
            ProductServiceCharge = serviceCharge,
            ProductDeliveryCharge = deliveryCharge,
            TotalAmount = totalAmount,
            Status = "Initiated",
            Signature = signature
        };

        _context.EsewaPayments.Add(payment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch when (!saleId.HasValue)
        {
            // Direct demo payments can still redirect to eSewa when a local database is unavailable.
        }

        return new EsewaPaymentInitiationDto
        {
            SaleId = saleId ?? 0,
            FormAction = _options.FormUrl,
            Fields = new Dictionary<string, string>
            {
                ["amount"] = FormatAmount(amount),
                ["tax_amount"] = FormatAmount(taxAmount),
                ["total_amount"] = FormatAmount(totalAmount),
                ["transaction_uuid"] = transactionUuid,
                ["product_code"] = _options.ProductCode,
                ["product_service_charge"] = FormatAmount(serviceCharge),
                ["product_delivery_charge"] = FormatAmount(deliveryCharge),
                ["success_url"] = _options.SuccessUrl,
                ["failure_url"] = _options.FailureUrl,
                ["signed_field_names"] = SignedFieldNames,
                ["signature"] = signature
            }
        };
    }

    public async Task<EsewaPaymentVerificationDto> VerifySuccessAsync(string encodedData)
    {
        if (string.IsNullOrWhiteSpace(encodedData))
            return Failed("Missing eSewa response data.");

        using var document = DecodeResponse(encodedData);
        var root = document.RootElement;
        var transactionUuid = GetRequiredString(root, "transaction_uuid");
        var status = GetRequiredString(root, "status");
        var productCode = GetRequiredString(root, "product_code");
        var receivedSignature = GetRequiredString(root, "signature");

        var expectedSignature = GenerateResponseSignature(root);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(receivedSignature),
                Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return Failed("Invalid eSewa response signature.", transactionUuid, status);
        }

        EsewaPayment? payment;
        try
        {
            payment = await _context.EsewaPayments
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(p => p.TransactionUuid == transactionUuid);
        }
        catch when (transactionUuid.StartsWith("DIRECT-", StringComparison.OrdinalIgnoreCase))
        {
            return new EsewaPaymentVerificationDto
            {
                Success = status == "COMPLETE",
                TransactionUuid = transactionUuid,
                TransactionCode = GetOptionalString(root, "transaction_code")
                    ?? GetOptionalString(root, "ref_id"),
                Status = status,
                Message = status == "COMPLETE"
                    ? "eSewa payment verified successfully."
                    : $"eSewa returned status {status}."
            };
        }

        if (payment == null)
            return Failed("Payment transaction was not found.", transactionUuid, status);

        if (productCode != payment.ProductCode)
            return Failed("Invalid eSewa product code.", transactionUuid, status, payment.SaleId);

        payment.TransactionCode = GetOptionalString(root, "transaction_code")
            ?? GetOptionalString(root, "ref_id");
        payment.Status = status;
        payment.Signature = receivedSignature;
        payment.VerifiedAt = DateTime.UtcNow;

        if (payment.Sale != null)
        {
            payment.Sale.PaymentStatus = status == "COMPLETE"
                ? PaymentStatus.Paid
                : PaymentStatus.Pending;
        }

        await _context.SaveChangesAsync();

        return new EsewaPaymentVerificationDto
        {
            Success = status == "COMPLETE",
            SaleId = payment.SaleId,
            TransactionUuid = transactionUuid,
            TransactionCode = payment.TransactionCode,
            Status = status,
            Message = status == "COMPLETE"
                ? "eSewa payment verified successfully."
                : $"eSewa returned status {status}."
        };
    }

    public async Task<EsewaPaymentVerificationDto> MarkFailureAsync(string? encodedData)
    {
        if (string.IsNullOrWhiteSpace(encodedData))
            return Failed("eSewa payment failed or was cancelled.");

        using var document = DecodeResponse(encodedData);
        var root = document.RootElement;
        var transactionUuid = GetOptionalString(root, "transaction_uuid") ?? "";
        var status = GetOptionalString(root, "status") ?? "FAILED";

        EsewaPayment? payment = null;

        try
        {
            payment = await _context.EsewaPayments
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(p => p.TransactionUuid == transactionUuid);
        }
        catch when (transactionUuid.StartsWith("DIRECT-", StringComparison.OrdinalIgnoreCase))
        {
            return Failed("eSewa payment failed or was cancelled.", transactionUuid, status);
        }

        if (payment != null)
        {
            payment.Status = status;
            payment.VerifiedAt = DateTime.UtcNow;

            if (payment.Sale != null)
                payment.Sale.PaymentStatus = PaymentStatus.Pending;

            await _context.SaveChangesAsync();
        }

        return Failed("eSewa payment failed or was cancelled.", transactionUuid, status, payment?.SaleId);
    }

    private string GenerateRequestSignature(decimal totalAmount, string transactionUuid)
    {
        var message = $"total_amount={FormatAmount(totalAmount)},transaction_uuid={transactionUuid},product_code={_options.ProductCode}";
        return GenerateSignature(message);
    }

    private string GenerateResponseSignature(JsonElement root)
    {
        var signedFieldNames = GetRequiredString(root, "signed_field_names");
        var message = string.Join(
            ",",
            signedFieldNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(field => $"{field}={GetRequiredString(root, field)}"));

        return GenerateSignature(message);
    }

    private string GenerateSignature(string message)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToBase64String(hmac.ComputeHash(messageBytes));
    }

    private static JsonDocument DecodeResponse(string encodedData)
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
        return JsonDocument.Parse(json);
    }

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.##", CultureInfo.InvariantCulture);

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            throw new Exception($"eSewa response is missing {propertyName}.");

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.Number => value.GetRawText(),
            _ => value.ToString()
        };
    }

    private static string? GetOptionalString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;

        return value.ValueKind == JsonValueKind.Null ? null : GetRequiredString(root, propertyName);
    }

    private static EsewaPaymentVerificationDto Failed(
        string message,
        string transactionUuid = "",
        string status = "FAILED",
        int? saleId = null) =>
        new()
        {
            Success = false,
            SaleId = saleId,
            TransactionUuid = transactionUuid,
            Status = status,
            Message = message
        };
}
