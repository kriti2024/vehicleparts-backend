using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public AdminReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("financial-summary")]
    public async Task<IActionResult> GetFinancialSummary()
    {
        var result = await _reportService.GetFinancialSummaryAsync();
        return Ok(result);
    }
}