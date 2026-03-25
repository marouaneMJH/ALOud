using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALOud.Services;

// SMTP implementation of IEmailService (reads configuration).
public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly SmtpOptions _smtpOptions;


    public SmtpEmailService(
        IConfiguration config,
        ILogger<SmtpEmailService> logger,
        IOptions<SmtpOptions> smtpOptions
    )
    {
        _logger = logger;
        _smtpOptions = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_smtpOptions.User) || string.IsNullOrEmpty(_smtpOptions.Password) || string.IsNullOrEmpty(_smtpOptions.From))
        {
            _logger.LogError("SMTP configuration is missing. Cannot send email.");

            throw new InvalidOperationException("SMTP configuration is missing");
        }

        var message = new MailMessage();
        message.From = new MailAddress(_smtpOptions.From);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpOptions.User, _smtpOptions.Password)
        };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

}
