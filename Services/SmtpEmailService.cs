using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        // var user = _config["SMTP_USER"] ?? Environment.GetEnvironmentVariable("SMTP_USER");
        // var pass = _config["SMTP_PASSWORD"] ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        // var from = _config["SMTP_FROM"] ?? Environment.GetEnvironmentVariable("SMTP_FROM");

        // if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(from))
        // {
        //     _logger.LogError("SMTP configuration is missing. Cannot send email.");
        //     _logger.LogError(Environment.GetEnvironmentVariable("SMTP_USER"));
        //     throw new InvalidOperationException("SMTP configuration is missing");
        // }

        // var message = new MailMessage();
        // message.From = new MailAddress(from);
        // message.To.Add(new MailAddress(to));
        // message.Subject = subject;
        // message.Body = htmlBody;
        // message.IsBodyHtml = true;

        // using var client = new SmtpClient("smtp.gmail.com", 587)
        // {
        //     EnableSsl = true,
        //     Credentials = new NetworkCredential(user, pass)
        // };

        // try
        // {
        //     await client.SendMailAsync(message);
        //     _logger.LogInformation("Email sent to {To}", to);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Failed to send email to {To}", to);
        //     throw;
        // }
    }
}
