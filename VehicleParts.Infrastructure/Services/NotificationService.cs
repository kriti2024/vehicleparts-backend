using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.Interfaces;
using VehicleParts.Domain.Entities;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyLowStockAsync(int partId, string partName, int currentStock)
    {
        var message =
            $"{partName} is low on stock. Current quantity: {currentStock}. Please restock soon.";

        var alreadyOpen = await _context.Notifications.AnyAsync(notification =>
            notification.Type == "LowStock" &&
            notification.PartId == partId &&
            !notification.IsRead);

        if (!alreadyOpen)
        {
            _context.Notifications.Add(new Notification
            {
                Type = "LowStock",
                PartId = partId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        var adminEmail =
            _configuration["AdminSettings:NotificationEmail"] ??
            _configuration["EmailSettings:AdminEmail"] ??
            _configuration["EmailSettings:Email"];

        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        try
        {
            await _emailService.SendEmailAsync(
                adminEmail,
                "Low Stock Alert",
                message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Low stock email could not be sent.");
        }
    }

    public async Task SendUnpaidCreditRemindersAsync()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var overdueSales = await _context.Sales
            .Include(sale => sale.Customer)
            .Where(sale =>
                (sale.PaymentStatus == PaymentStatus.Pending ||
                 sale.PaymentStatus == PaymentStatus.Credit) &&
                sale.SaleDate <= oneMonthAgo)
            .ToListAsync();

        foreach (var sale in overdueSales)
        {
            if (string.IsNullOrWhiteSpace(sale.Customer?.Email))
                continue;

            try
            {
                await _emailService.SendEmailAsync(
                    sale.Customer.Email,
                    "Unpaid Credit Reminder",
                    $"Dear {sale.Customer.FullName}, this is a reminder that your unpaid credit of Rs. {sale.FinalAmount:N2} from {sale.SaleDate:yyyy-MM-dd} is more than one month old. Please settle the payment at your earliest convenience.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Credit reminder email could not be sent for sale {SaleId}.",
                    sale.SaleId);
            }
        }
    }

    public async Task<List<Notification>> GetAdminNotificationsAsync()
    {
        return await _context.Notifications
            .Include(notification => notification.Part)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        if (notification == null)
            return;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }
}
