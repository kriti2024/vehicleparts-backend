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
        var token = await authService
            .LoginAsync(dto.Email, dto.Password);

        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        await authService.RegisterAsync(dto.Email, dto.Password, dto.FullName);
        return Ok(new { message = "Registration successful" });
    }
}