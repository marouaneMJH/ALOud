using System.Security.Cryptography;
using ALOud.Data;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

public class VerificationService : IVerificationService
{
    // Generates and validates email OTP codes and marks users verified.
    private readonly ALOudDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<VerificationService> _logger;
    private readonly IConfiguration _configuration;

    public VerificationService(
        ALOudDbContext db, 
        IEmailService email, 
        ILogger<VerificationService> logger,
        IConfiguration configuration)
    {
        _db = db;
        _email = email;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendVerificationAsync(User user)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
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

        await SendVerificationEmailAsync(user, code);
    }

    public async Task<bool> ResendVerificationAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            _logger.LogWarning("Attempted to resend verification for non-existent email: {Email}", email);
            return false;
        }

        if (user.IsEmailVerified)
        {
            _logger.LogWarning("Attempted to resend verification for already verified email: {Email}", email);
            return false;
        }

        // Invalidate any existing unused verification codes
        var existingCodes = await _db.EmailVerifications
            .Where(v => v.UserId == user.Id && !v.IsUsed)
            .ToListAsync();

        foreach (var oldCode in existingCodes)
        {
            oldCode.IsUsed = true;
        }

        // Generate new code
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
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

        await SendVerificationEmailAsync(user, code);
        
        _logger.LogInformation("Verification code resent to {Email}", email);
        return true;
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

    private async Task SendVerificationEmailAsync(User user, string code)
    {
        // Get base URL from configuration or construct it
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost";
        var verificationLink = $"{baseUrl}/Account/Verify?email={Uri.EscapeDataString(user.Email)}&code={code}";

        var subject = "Welcome to ALOud - Verify Your Email";
        var body = $@"
            <div style=""font-family: 'Inter', Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 0;"">
                <!-- Header with ALOud branding -->
                <div style=""background-color: #0b0b0b; padding: 40px 20px; text-align: center; border-radius: 0;"">
                    <h1 style=""font-family: 'Playfair Display', Georgia, serif; color: #c9a24d; margin: 0; font-size: 32px; font-weight: 400; letter-spacing: 2px;"">ALOud</h1>
                    <p style=""color: #c9a24d; margin: 8px 0 0 0; font-size: 12px; letter-spacing: 1px; text-transform: uppercase;"">The Art of Authentic Perfumery</p>
                </div>
                
                <!-- Body content -->
                <div style=""background-color: #f7f7f5; padding: 40px 30px; border-radius: 0;"">
                    <p style=""color: #0b0b0b; font-size: 16px; margin: 0 0 8px 0; line-height: 1.6;"">Hello <strong>{user.FirstName}</strong>,</p>
                    
                    <p style=""color: #2a2a2a; font-size: 15px; line-height: 1.8; margin: 20px 0;"">
                        Thank you for joining the ALOud family. To complete your registration and activate your account, please verify your email address using the code below:
                    </p>
                    
                    <!-- Verification Code Display -->
                    <div style=""background-color: #ffffff; border: 2px solid #c9a24d; border-radius: 8px; padding: 25px; margin: 30px 0; text-align: center;"">
                        <p style=""color: #2a2a2a; font-size: 12px; margin: 0 0 12px 0; text-transform: uppercase; letter-spacing: 1px;"">Your Verification Code</p>
                        <div style=""font-family: 'Courier New', monospace; font-size: 36px; font-weight: 700; color: #c9a24d; letter-spacing: 8px; margin: 0; word-break: break-all;"">{code}</div>
                    </div>
                    
                    <!-- Divider -->
                    <p style=""margin: 30px 0; color: #2a2a2a; text-align: center; font-size: 14px;"">or</p>
                    
                    <!-- Verification Button -->
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{verificationLink}"" 
                           style=""display: inline-block; background-color: #0b0b0b; color: #c9a24d; padding: 14px 50px; text-decoration: none; border-radius: 0; 
                                  font-weight: 600; font-size: 14px; letter-spacing: 1px; text-transform: uppercase; border: 2px solid #0b0b0b; transition: all 0.3s ease;"">
                            Verify Email Address
                        </a>
                    </div>
                    
                    <!-- Additional Instructions -->
                    <div style=""background-color: #ffffff; border-left: 3px solid #c9a24d; padding: 15px 20px; margin: 30px 0; border-radius: 0;"">
                        <p style=""margin: 0; color: #0b0b0b; font-size: 13px; line-height: 1.6;"">
                            <strong>Instructions:</strong>
                        </p>
                        <ul style=""margin: 8px 0 0 0; padding-left: 20px; color: #2a2a2a; font-size: 13px; line-height: 1.8;"">
                            <li>Click the button above, or</li>
                            <li>Copy and paste the code into the verification form, or</li>
                            <li>Click the link in the email if available</li>
                        </ul>
                    </div>
                    
                    <!-- Expiration Notice -->
                    <p style=""color: #2a2a2a; font-size: 12px; margin: 25px 0; padding: 12px; background-color: #ffffff; border: 1px solid #e5e5e3; border-radius: 0;"">
                        ⏱️ <strong>This code will expire in 15 minutes.</strong> If it expires, you can request a new one.
                    </p>
                    
                    <!-- Support and Footer -->
                    <p style=""color: #2a2a2a; font-size: 13px; line-height: 1.6; margin: 30px 0 0 0;"">
                        If you did not create this account, please ignore this email. Your email address will remain secure.
                    </p>
                </div>
                
                <!-- Footer -->
                <div style=""background-color: #0b0b0b; padding: 30px 20px; text-align: center; border-radius: 0;"">
                    <p style=""color: #c9a24d; font-size: 12px; margin: 0 0 8px 0; letter-spacing: 0.5px;"">ALOud - The Art of Authentic Perfumery</p>
                    <p style=""color: #666; font-size: 11px; margin: 0;"">&copy; @DateTime.UtcNow.Year ALOud. All rights reserved.</p>
                </div>
            </div>";

        await _email.SendEmailAsync(user.Email, subject, body);
        _logger.LogInformation("Sent verification code to {Email}", user.Email);
    }
}
