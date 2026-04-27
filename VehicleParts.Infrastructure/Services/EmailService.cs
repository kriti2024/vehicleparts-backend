using System.Diagnostics;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Mock email sending - Log to console/debug
        Debug.WriteLine($"[EMAIL SENT] To: {to} | Subject: {subject} | Body: {body}");
        Console.WriteLine($"[EMAIL SENT] To: {to} | Subject: {subject} | Body: {body}");
        return Task.CompletedTask;
    }
}
