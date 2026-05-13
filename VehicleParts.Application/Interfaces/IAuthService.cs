using VehicleParts.Application.DTOs;
using VehicleParts.Application.DTOs.Auth;

namespace VehicleParts.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
