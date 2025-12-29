using ALOud.Models;

namespace ALOud.Services;

public interface IVerificationService
{
    Task SendVerificationAsync(User user);
    Task<bool> VerifyCodeAsync(string email, string code);
}
