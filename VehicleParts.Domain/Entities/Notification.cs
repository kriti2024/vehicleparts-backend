using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Domain.Entities;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "LowStock"; // e.g., LowStock, PendingCredit

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public int? PartId { get; set; }
    public Part? Part { get; set; }
}
