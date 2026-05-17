using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;

    public NotificationController(
        INotificationService notificationService,
        IApplicationDbContext context)
    {
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdminNotifications()
    {
        var notifications = await _notificationService.GetAdminNotificationsAsync();

        return Ok(notifications.Select(notification => new
        {
            notification.NotificationId,
            notification.Message,
            notification.Type,
            notification.CreatedAt,
            notification.IsRead,
            notification.PartId,
            PartName = notification.Part?.PartName
        }));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPost("send-credit-reminders")]
    public async Task<IActionResult> SendCreditReminders()
    {
        await _notificationService.SendUnpaidCreditRemindersAsync();
        return Ok(new { message = "Credit reminders processed successfully." });
    }

    [HttpPost("process-low-stock")]
    public async Task<IActionResult> ProcessLowStockAlerts([FromQuery] int threshold = 10)
    {
        var lowStockParts = await _context.Parts
            .Where(part => part.StockQuantity < threshold)
            .ToListAsync();

        foreach (var part in lowStockParts)
        {
            await _notificationService.NotifyLowStockAsync(
                part.PartId,
                part.PartName,
                part.StockQuantity);
        }

        return Ok(new
        {
            message = $"{lowStockParts.Count} low stock alert(s) processed.",
            count = lowStockParts.Count
        });
    }
}
