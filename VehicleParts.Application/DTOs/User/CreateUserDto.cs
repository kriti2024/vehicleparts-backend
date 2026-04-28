using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs;

public class CreateUserDto : RegisterDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}