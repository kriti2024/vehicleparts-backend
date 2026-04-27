using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public NotificationService(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task NotifyLowStockAsync(int partId, string partName, int currentStock)
    {
        // For Point 15: Notify Admin for low stock (<10)
        // In a real app, this might go to a notification table or an admin dashboard websocket
        // Here we'll simulate by sending an email to a configured admin email
        await _emailService.SendEmailAsync(
            "admin@vehicleparts.com", 
            "Low Stock Alert", 
            $"Alert: Part '{partName}' (ID: {partId}) is low on stock. Current quantity: {currentStock}. Please restock soon.");
    }

    public async Task SendUnpaidCreditRemindersAsync()
    {
        // For Point 15: email reminders to customers with unpaid credits for more than 1 month
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var overdueSales = await _context.Sales
            .Include(s => s.Customer)
            .Where(s => (s.PaymentStatus == Domain.Enums.PaymentStatus.Pending || s.PaymentStatus == Domain.Enums.PaymentStatus.Credit)
                        && s.SaleDate <= oneMonthAgo)
            .ToListAsync();

        foreach (var sale in overdueSales)
        {
            if (sale.Customer != null && !string.IsNullOrEmpty(sale.Customer.Email))
            {
                await _emailService.SendEmailAsync(
                    sale.Customer.Email,
                    "Unpaid Credit Reminder",
                    $"Dear {sale.Customer.FullName}, this is a reminder that you have an unpaid balance of {sale.FinalAmount:C} from your purchase on {sale.SaleDate:d}. Please settle your payment at your earliest convenience.");
            }
        }
    }
}
