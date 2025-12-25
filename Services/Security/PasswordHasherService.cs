using Microsoft.AspNetCore.Identity;

namespace ALOud.Services.Security;

public class PasswordHasherService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
           == PasswordVerificationResult.Success;
}