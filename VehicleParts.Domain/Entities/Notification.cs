using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "LowStock";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public int? PartId { get; set; }
    public Part? Part { get; set; }
}
