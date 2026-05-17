using VehicleParts.Application.DTOs.Payments;

namespace VehicleParts.Application.Interfaces;

public interface IEsewaPaymentService
{
    Task<EsewaPaymentInitiationDto> InitiatePaymentAsync(
        int saleId,
        EsewaPaymentRequestDto request);

    Task<EsewaPaymentInitiationDto> InitiateDirectPaymentAsync(
        EsewaPaymentRequestDto request);

    Task<EsewaPaymentVerificationDto> VerifySuccessAsync(string encodedData);

    Task<EsewaPaymentVerificationDto> MarkFailureAsync(string? encodedData);
}
