namespace VehicleParts.Infrastructure.Services;

public class EsewaOptions
{
    public string FormUrl { get; set; } = "https://rc-epay.esewa.com.np/api/epay/main/v2/form";
    public string ProductCode { get; set; } = "EPAYTEST";
    public string SecretKey { get; set; } = "8gBm/:&EnhH.1/q";
    public string SuccessUrl { get; set; } = "https://localhost:7000/api/esewa/success";
    public string FailureUrl { get; set; } = "https://localhost:7000/api/esewa/failure";
    public string FrontendSuccessUrl { get; set; } = "http://localhost:5173/customer/payments";
    public string FrontendFailureUrl { get; set; } = "http://localhost:5173/customer/payments";
}
