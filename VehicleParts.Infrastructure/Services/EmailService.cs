using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using VehicleParts.Application.Interfaces;

namespace VehicleParts.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var sender = _configuration["EmailSettings:Email"];
        var password = _configuration["EmailSettings:Password"];
        var host = _configuration["EmailSettings:Host"];
        var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

        if (string.IsNullOrWhiteSpace(sender) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Email SMTP settings are not configured.");
        }

        using var message = new MailMessage(sender, to, subject, body);
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(sender, password)
        };

        await client.SendMailAsync(message);
    }
}
