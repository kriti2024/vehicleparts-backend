using VehicleParts.Domain.Entities;

namespace VehicleParts.Application.Interfaces;

public interface INotificationService
{
    Task NotifyLowStockAsync(int partId, string partName, int currentStock);
    Task SendUnpaidCreditRemindersAsync();
    Task<List<Notification>> GetAdminNotificationsAsync();
    Task MarkAsReadAsync(int notificationId);
}
