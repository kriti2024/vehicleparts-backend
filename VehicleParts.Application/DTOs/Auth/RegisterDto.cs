using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    // Vehicle Info
    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string VehicleModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? VehicleBrand { get; set; }

    public int? VehicleYear { get; set; }
}