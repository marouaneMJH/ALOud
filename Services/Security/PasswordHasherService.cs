using Microsoft.AspNetCore.Identity;

namespace ALOud.Services.Security;

// Lightweight wrapper for ASP.NET Core Identity password hashing.
public class PasswordHasherService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
           == PasswordVerificationResult.Success;
}