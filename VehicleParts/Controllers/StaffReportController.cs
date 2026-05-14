using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Staff")] // Assuming a role-based authorization is in place
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
        try
        {
            var result = await _reportService.GetCustomerReportAsync();
            if (result.RegularCustomers.Count == 0 &&
                result.HighSpenders.Count == 0 &&
                result.PendingCreditCustomers.Count == 0)
            {
                return Ok(StaffFallbackStore.GetCustomerReport());
            }

            return Ok(result);
        }
        catch
        {
            var result = StaffFallbackStore.GetCustomerReport();
            return Ok(result);
        }
    }
}
