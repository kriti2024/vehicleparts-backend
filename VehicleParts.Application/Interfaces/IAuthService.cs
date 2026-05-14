using VehicleParts.Application.DTOs;

namespace VehicleParts.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(string email, string password);
}
