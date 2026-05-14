using Microsoft.AspNetCore.Mvc;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/staff-operations")]
public class StaffOperationsController : ControllerBase
{
    private static readonly List<ServiceJobDto> ServiceJobs =
    [
        new(
            "JOB-1048",
            "Walk-in customer",
            "BA 12 PA 1234",
            "Brake inspection",
            DateTime.UtcNow.Date,
            "High",
            "In Progress",
            "Check front brake pads before invoice."),
        new(
            "JOB-1049",
            "Regular customer",
            "BAG 4412",
            "Oil change",
            DateTime.UtcNow.Date,
            "Normal",
            "Waiting",
            "Use 5W-30 synthetic oil.")
    ];

    private static readonly List<CustomerCreditDto> Credits =
    [
        new(
            "CR-221",
            "Regular Customer",
            "9800000000",
            "customer@email.com",
            8500,
            DateTime.UtcNow.Date,
            "Pending",
            "Call after 4 PM."),
        new(
            "CR-222",
            "Fleet Buyer",
            "9811111111",
            "fleet@email.com",
            12400,
            DateTime.UtcNow.Date,
            "Promised",
            "Promised to clear after invoice approval.")
    ];

    private static readonly List<StockAlertDto> StockAlerts =
    [
        new("P-101", "Brake Pad Set", "Brakes", 4, 8, "Axleworks Stock", "Low"),
        new("P-102", "Engine Oil 5W-30", "Fluids", 18, 10, "Axleworks Stock", "OK"),
        new("P-104", "Air Filter", "Filters", 2, 6, "Axleworks Stock", "Critical"),
        new("P-107", "Car Battery", "Electrical", 6, 5, "Axleworks Stock", "OK")
    ];

    [HttpGet("service-queue")]
    public ActionResult<IEnumerable<ServiceJobDto>> GetServiceQueue()
    {
        return Ok(ServiceJobs.OrderByDescending(job => job.Date).ThenBy(job => job.Status));
    }

    [HttpPost("service-queue")]
    public ActionResult<ServiceJobDto> CreateServiceJob([FromBody] CreateServiceJobDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Customer) ||
            string.IsNullOrWhiteSpace(request.Vehicle) ||
            string.IsNullOrWhiteSpace(request.Service))
        {
            return BadRequest(new { message = "Customer, vehicle, and service are required." });
        }

        var job = new ServiceJobDto(
            $"JOB-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()[^4..]}",
            request.Customer.Trim(),
            request.Vehicle.Trim(),
            request.Service.Trim(),
            request.Date.Date,
            string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            "Waiting",
            string.IsNullOrWhiteSpace(request.Notes) ? "No extra notes." : request.Notes.Trim());

        ServiceJobs.Insert(0, job);

        return CreatedAtAction(nameof(GetServiceQueue), new { id = job.Id }, job);
    }

    [HttpPatch("service-queue/{id}/status")]
    public ActionResult<ServiceJobDto> UpdateServiceJobStatus(string id, [FromBody] UpdateStatusDto request)
    {
        var index = ServiceJobs.FindIndex(job => job.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return NotFound(new { message = $"Service job {id} was not found." });
        }

        var current = ServiceJobs[index];
        var updated = current with { Status = request.Status };
        ServiceJobs[index] = updated;

        return Ok(updated);
    }

    [HttpGet("credits")]
    public ActionResult<IEnumerable<CustomerCreditDto>> GetCredits()
    {
        return Ok(Credits.OrderByDescending(credit => credit.Status != "Paid").ThenByDescending(credit => credit.Amount));
    }

    [HttpPatch("credits/{id}")]
    public ActionResult<CustomerCreditDto> UpdateCredit(string id, [FromBody] UpdateCreditDto request)
    {
        var index = Credits.FindIndex(credit => credit.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return NotFound(new { message = $"Credit account {id} was not found." });
        }

        var current = Credits[index];
        var updated = current with
        {
            Status = string.IsNullOrWhiteSpace(request.Status) ? current.Status : request.Status.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? current.Note : request.Note.Trim()
        };
        Credits[index] = updated;

        return Ok(updated);
    }

    [HttpGet("stock-alerts")]
    public ActionResult<IEnumerable<StockAlertDto>> GetStockAlerts([FromQuery] string? status = null)
    {
        var alerts = StockAlerts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            alerts = alerts.Where(item => item.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(alerts.OrderBy(item => item.Status == "Critical" ? 0 : item.Status == "Low" ? 1 : 2));
    }

    [HttpPatch("stock-alerts/{id}/restock")]
    public ActionResult<StockAlertDto> RestockItem(string id)
    {
        var index = StockAlerts.FindIndex(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return NotFound(new { message = $"Stock item {id} was not found." });
        }

        var current = StockAlerts[index];
        var updated = current with
        {
            Stock = current.ReorderLevel + 6,
            Status = "OK"
        };
        StockAlerts[index] = updated;

        return Ok(updated);
    }
}

public record ServiceJobDto(
    string Id,
    string Customer,
    string Vehicle,
    string Service,
    DateTime Date,
    string Priority,
    string Status,
    string Notes);

public record CreateServiceJobDto(
    string Customer,
    string Vehicle,
    string Service,
    DateTime Date,
    string Priority,
    string? Notes);

public record UpdateStatusDto(string Status);

public record CustomerCreditDto(
    string Id,
    string Customer,
    string Phone,
    string Email,
    decimal Amount,
    DateTime DueDate,
    string Status,
    string Note);

public record UpdateCreditDto(string Status, string? Note);

public record StockAlertDto(
    string Id,
    string Part,
    string Category,
    int Stock,
    int ReorderLevel,
    string Vendor,
    string Status);
