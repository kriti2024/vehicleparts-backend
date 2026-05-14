using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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
        try
        {
            Console.WriteLine("===== EMAIL PROCESS STARTED =====");

            var email = _configuration["EmailSettings:Email"]
                ?? throw new InvalidOperationException("Email sender address is not configured.");
            var password = _configuration["EmailSettings:Password"]
                ?? throw new InvalidOperationException("Email password is not configured.");
            var host = _configuration["EmailSettings:Host"]
                ?? throw new InvalidOperationException("Email SMTP host is not configured.");
            var port = int.Parse(_configuration["EmailSettings:Port"]!);

            Console.WriteLine($"Sender: {email}");
            Console.WriteLine($"Receiver: {to}");
            Console.WriteLine($"SMTP Host: {host}");
            Console.WriteLine($"Port: {port}");

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "Vehicle Parts System",
                    email
                )
            );

            message.To.Add(
                MailboxAddress.Parse(to)
            );

            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new SmtpClient();

            Console.WriteLine("Connecting SMTP...");
            await client.ConnectAsync(
                host,
                port,
                SecureSocketOptions.StartTls
            );

            Console.WriteLine("Connected.");

            Console.WriteLine("Authenticating...");
            await client.AuthenticateAsync(
                email,
                password
            );

            Console.WriteLine("Authenticated.");

            Console.WriteLine("Sending Email...");
            await client.SendAsync(message);

            Console.WriteLine("Email Sent Successfully.");

            await client.DisconnectAsync(true);

            Console.WriteLine("Disconnected.");
            Console.WriteLine("===== EMAIL PROCESS FINISHED =====");
        }
        catch (Exception ex)
        {
            Console.WriteLine("===== EMAIL ERROR =====");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }
}
