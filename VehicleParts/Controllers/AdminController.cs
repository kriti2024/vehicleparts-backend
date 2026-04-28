using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController(IUserService userService) : ControllerBase
{
    // POST: api/admin/staff
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff(CreateStaffDto dto)
    {
        var result = await userService.CreateStaffAsync(dto);

        return Ok(result);
    }

    // GET: api/admin/staff
    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff()
    {
        var result = await userService.GetAllStaffAsync();

        return Ok(result);
    }

    // DELETE: api/admin/staff/{id}
    [HttpDelete("staff/{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id)
    {
        await userService.DeleteStaffAsync(id);

        return Ok(new { message = "Staff deleted successfully." });
    }

    // PUT: api/admin/staff/{id}/role
    [HttpPut("staff/{id}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeRoleDto dto)
    {
        await userService.ChangeRoleAsync(id, dto.Role);

        return Ok(new { message = "Role updated successfully." });
    }
}