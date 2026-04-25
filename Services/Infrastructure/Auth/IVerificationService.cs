using ALOud.Models;

namespace ALOud.Services;

// Email verification: send OTP and validate codes.
public interface IVerificationService
{
    Task SendVerificationAsync(User user);
    Task<bool> VerifyCodeAsync(string email, string code);
    Task<bool> ResendVerificationAsync(string email);
}
