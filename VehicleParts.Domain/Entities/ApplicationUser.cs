using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}