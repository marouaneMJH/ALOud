using ALOud.Data;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

public class VerificationService : IVerificationService
{
    private readonly ALOudDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(ALOudDbContext db, IEmailService email, ILogger<VerificationService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task SendVerificationAsync(User user)
    {
        var code = new Random().Next(100000, 999999).ToString();
        var expires = DateTime.UtcNow.AddMinutes(15);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = expires,
            IsUsed = false
        };

        _db.EmailVerifications.Add(verification);
        await _db.SaveChangesAsync();

        var subject = "Your ALOud verification code";
        var body = $"<p>Hello {user.FirstName},</p><p>Your verification code is: <strong>{code}</strong></p><p>This code expires in 15 minutes.</p>";

        await _email.SendEmailAsync(user.Email, subject, body);
        _logger.LogInformation("Sent verification code to {Email}", user.Email);
    }

    public async Task<bool> VerifyCodeAsync(string email, string code)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var verification = await _db.EmailVerifications
            .Where(v => v.UserId == user.Id && !v.IsUsed && v.Code == code)
            .OrderByDescending(v => v.ExpiresAt)
            .FirstOrDefaultAsync();

        if (verification == null) return false;
        if (verification.ExpiresAt < DateTime.UtcNow) return false;

        verification.IsUsed = true;
        user.IsEmailVerified = true;

        await _db.SaveChangesAsync();
        return true;
    }
}
