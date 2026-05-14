using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Constants;

namespace VehicleParts.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService)
    : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
    {
        var result =
            await authService.RegisterAsync(dto);

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var result =
            await authService.LoginAsync(dto);

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me(
        [FromServices]
        ICurrentUserService currentUser)
    {
        return Ok(new
        {
            userId = currentUser.UserId,
            email = currentUser.Email,
            isAuthenticated =
                currentUser.IsAuthenticated,

            isAdmin =
                currentUser.IsInRole(
                    Roles.Admin),

            isStaff =
                currentUser.IsInRole(
                    Roles.Staff),

            isCustomer =
                currentUser.IsInRole(
                    Roles.Customer)
        });
    }
}
