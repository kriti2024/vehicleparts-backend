using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send-credit-reminders")]
    public async Task<IActionResult> SendCreditReminders()
    {
        // Can be called by a scheduled job or manually by admin
        await _notificationService.SendUnpaidCreditRemindersAsync();
        return Ok(new { message = "Credit reminders processed successfully" });
    }
}
