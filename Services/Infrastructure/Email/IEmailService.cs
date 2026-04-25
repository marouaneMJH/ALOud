namespace ALOud.Services;
// Abstraction for sending emails (SMTP or other providers).
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
