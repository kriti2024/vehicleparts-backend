using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.CustomerProfile;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/customer-profile")]
[Authorize(Roles = "Admin,Staff,Customer")]
public class CustomerProfileController(ICustomerProfileService customerProfileService) : ControllerBase
{
    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<CustomerProfileDetailsDto>> GetProfile(int customerId)
    {
        var result = await customerProfileService.GetProfileAsync(customerId);

        if (result == null)
            return NotFound(new { message = "Customer not found." });

        return Ok(result);
    }

    [HttpPut("{customerId:int}")]
    public async Task<ActionResult<CustomerProfileDetailsDto>> UpdateProfile(
        int customerId,
        [FromBody] UpdateCustomerProfileDto dto)
    {
        var result = await customerProfileService.UpdateProfileAsync(customerId, dto);

        if (result == null)
            return NotFound(new { message = "Customer not found." });

        return Ok(result);
    }
}