using VehicleParts.Application.Interfaces;

namespace VehicleParts.Application.Services;

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // For development/demonstration purposes, we just log the email to console
        Console.WriteLine($"[MOCK EMAIL] To: {to}");
        Console.WriteLine($"[MOCK EMAIL] Subject: {subject}");
        Console.WriteLine($"[MOCK EMAIL] Body: {body}");
        Console.WriteLine("------------------------------------------");
        return Task.CompletedTask;
    }
}
