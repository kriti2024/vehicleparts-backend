namespace VehicleParts.Application.Interfaces;

public interface INotificationService
{
    Task NotifyLowStockAsync(int partId, string partName, int currentStock);
    Task SendUnpaidCreditRemindersAsync();
}
