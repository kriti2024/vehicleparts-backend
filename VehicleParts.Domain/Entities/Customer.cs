using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }
}