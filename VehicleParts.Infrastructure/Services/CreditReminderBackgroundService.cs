using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Infrastructure.Services;

public class CreditReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CreditReminderBackgroundService> _logger;

    public CreditReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CreditReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService =
                    scope.ServiceProvider.GetRequiredService<INotificationService>();

                await notificationService.SendUnpaidCreditRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending credit reminders.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
