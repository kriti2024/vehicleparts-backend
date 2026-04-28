using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs;

public class ChangeRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}