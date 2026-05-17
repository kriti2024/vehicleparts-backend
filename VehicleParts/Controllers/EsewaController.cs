using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VehicleParts.Application.DTOs.Payments;
using VehicleParts.Application.Interfaces;
using VehicleParts.Infrastructure.Services;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EsewaController : ControllerBase
{
    private const string EsewaSignatureTestVector = "i94zsd3oXF6ZsSr/kGqT4sSzYQzjj1W/waxjWyRwaME=";
    private readonly IEsewaPaymentService _esewaPaymentService;
    private readonly EsewaOptions _options;

    public EsewaController(
        IEsewaPaymentService esewaPaymentService,
        IOptions<EsewaOptions> options)
    {
        _esewaPaymentService = esewaPaymentService;
        _options = options.Value;
    }

    [HttpPost("sales/{saleId}/initiate")]
    public async Task<ActionResult<EsewaPaymentInitiationDto>> InitiatePayment(
        int saleId,
        [FromBody] EsewaPaymentRequestDto request)
    {
        try
        {
            var payment = await _esewaPaymentService.InitiatePaymentAsync(saleId, request);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<EsewaPaymentInitiationDto>> InitiateDirectPayment(
        [FromBody] EsewaPaymentRequestDto request)
    {
        try
        {
            var payment = await _esewaPaymentService.InitiateDirectPaymentAsync(request);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("redirect")]
    public async Task<IActionResult> RedirectToEsewa([FromQuery] decimal amount)
    {
        if (_options.ProductCode == "EPAYTEST" && _options.SecretKey != "8gBm/:&EnhH.1/q")
        {
            return BadRequest(new
            {
                message = "Invalid eSewa UAT secret key. EPAYTEST must use the key that matches eSewa's signature test vector.",
                expectedTestSignature = EsewaSignatureTestVector
            });
        }

        try
        {
            var payment = await _esewaPaymentService.InitiateDirectPaymentAsync(
                new EsewaPaymentRequestDto
                {
                    Amount = amount,
                    TaxAmount = 0,
                    ProductServiceCharge = 0,
                    ProductDeliveryCharge = 0
                });

            return Content(BuildAutoSubmitForm(payment), "text/html");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery] string data)
    {
        var result = await _esewaPaymentService.VerifySuccessAsync(data);
        var url = BuildRedirectUrl(
            result.Success ? _options.FrontendSuccessUrl : _options.FrontendFailureUrl,
            result);

        return Redirect(url);
    }

    [HttpGet("failure")]
    public async Task<IActionResult> Failure([FromQuery] string? data)
    {
        var result = await _esewaPaymentService.MarkFailureAsync(data);
        var url = BuildRedirectUrl(_options.FrontendFailureUrl, result);

        return Redirect(url);
    }

    private static string BuildRedirectUrl(
        string baseUrl,
        EsewaPaymentVerificationDto result)
    {
        var separator = baseUrl.Contains('?') ? '&' : '?';
        var query = string.Join(
            "&",
            new Dictionary<string, string?>
            {
                ["payment"] = result.Success ? "success" : "failed",
                ["saleId"] = result.SaleId?.ToString(),
                ["transactionUuid"] = result.TransactionUuid,
                ["status"] = result.Status,
                ["message"] = result.Message
            }
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));

        return $"{baseUrl}{separator}{query}";
    }

    private static string BuildAutoSubmitForm(EsewaPaymentInitiationDto payment)
    {
        var inputs = string.Join(
            Environment.NewLine,
            payment.Fields.Select(field =>
                $"""<input type="hidden" name="{HtmlEncode(field.Key)}" value="{HtmlEncode(field.Value)}" />"""));

        return $$"""
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Redirecting to eSewa</title>
    <style>
        body {
            margin: 0;
            min-height: 100vh;
            display: grid;
            place-items: center;
            font-family: Arial, sans-serif;
            background: #f6f2ea;
            color: #211915;
        }

        main {
            width: min(420px, calc(100vw - 32px));
            border: 1px solid #e2d8ca;
            border-radius: 16px;
            background: #fffaf2;
            padding: 28px;
            box-shadow: 0 20px 50px rgba(40, 30, 20, 0.12);
        }

        h1 {
            margin: 0 0 10px;
            font-size: 24px;
        }

        p {
            margin: 0 0 20px;
            color: #6f6256;
            line-height: 1.5;
        }

        button {
            width: 100%;
            border: 0;
            border-radius: 12px;
            background: #f6a11a;
            color: #211915;
            cursor: pointer;
            font-weight: 700;
            padding: 14px 18px;
        }
    </style>
</head>
<body>
    <main>
        <h1>Redirecting to eSewa</h1>
        <p>Your payment is ready. If the eSewa page does not open automatically, continue manually.</p>
        <form id="esewa-payment-form" method="post" action="{{HtmlEncode(payment.FormAction)}}">
            {{inputs}}
            <button type="submit">Continue to eSewa</button>
        </form>
    </main>
    <script>
        window.setTimeout(function () {
            HTMLFormElement.prototype.submit.call(document.getElementById('esewa-payment-form'));
        }, 500);
    </script>
</body>
</html>
""";
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
