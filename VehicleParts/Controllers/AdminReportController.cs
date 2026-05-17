using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public AdminReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET: api/admin/reports/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _reportService.GetFinancialSummaryAsync();
        return Ok(result);
    }

    // GET: api/admin/reports/daily
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport()
    {
        var data = await _reportService.GetFinancialSummaryAsync();

        return Ok(new
        {
            Sales = data.DailySales,
            Invoices = data.DailyInvoices
        });
    }

    // GET: api/admin/reports/monthly
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport()
    {
        var data = await _reportService.GetFinancialSummaryAsync();

        return Ok(new
        {
            Sales = data.MonthlySales,
            Invoices = data.MonthlyInvoices
        });
    }

    // GET: api/admin/reports/monthly-revenue
    [HttpGet("monthly-revenue")]
    public async Task<IActionResult> GetMonthlyRevenue()
    {
        var result = await _reportService.GetMonthlyRevenueAsync();
        return Ok(result);
    }

    // GET: api/admin/reports/yearly
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlyReport()
    {
        var data = await _reportService.GetFinancialSummaryAsync();

        return Ok(new
        {
            Sales = data.YearlySales,
            Invoices = data.YearlyInvoices
        });
    }
}
