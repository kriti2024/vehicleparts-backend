using VehicleParts.Application.DTOs;

namespace VehicleParts.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateStaffAsync(CreateStaffDto dto);

    Task<IEnumerable<UserDto>> GetAllStaffAsync();

    Task DeleteStaffAsync(Guid id);

    Task ChangeRoleAsync(Guid id, string role);
}