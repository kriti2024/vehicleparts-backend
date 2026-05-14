using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await authService
            .LoginAsync(dto.Email, dto.Password);

        return Ok(response);
    }
}
