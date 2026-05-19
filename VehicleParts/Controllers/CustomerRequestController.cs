using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.CustomerRequests;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/customer-requests")]
public class CustomerRequestController(ICustomerRequestService requestService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PartRequestDto>> CreateRequest([FromBody] CreatePartRequestDto dto)
    {
        var result = await requestService.CreateRequestAsync(dto);
        return Ok(result);
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<List<PartRequestDto>>> GetCustomerRequests(int customerId)
    {
        var result = await requestService.GetCustomerRequestsAsync(customerId);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<PartRequestDto>>> GetAllRequests()
    {
        var result = await requestService.GetAllRequestsAsync();
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<PartRequestDto>> UpdateRequestStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var result = await requestService.UpdateRequestStatusAsync(id, dto.Status);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}
