using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff")] //  role-based authorization is in place
public class StaffReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public StaffReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("customer-reports")]
    public async Task<IActionResult> GetCustomerReports()
    {
        var result = await _reportService.GetCustomerReportAsync();
        return Ok(result);
    }
}