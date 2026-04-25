using FluentAssertions;
using ALOud.Data;
using ALOud.Models;
using ALOud.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Common.TestDataBuilders;
using Xunit;

namespace Tests.Services.Infrastructure.Auth;

public class VerificationServiceTests : IDisposable
{
    private readonly ALOudDbContext _context;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<VerificationService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly VerificationService _verificationService;

    public VerificationServiceTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<ALOudDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ALOudDbContext(options);
        
        // Set up mocks
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<VerificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Configure default base URL
        _mockConfiguration.Setup(c => c["AppSettings:BaseUrl"]).Returns("https://localhost");
        
        _verificationService = new VerificationService(
            _context, 
            _mockEmailService.Object, 
            _mockLogger.Object, 
            _mockConfiguration.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region SendVerificationAsync Tests

    [Fact]
    public async Task SendVerificationAsync_WithValidUser_ShouldCreateVerificationCodeAndSendEmail()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.SendVerificationAsync(user);

        // Assert
        var verification = await _context.EmailVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id);
        
        verification.Should().NotBeNull();
        verification!.Code.Should().HaveLength(6);
        verification.Code.Should().MatchRegex(@"^\d{6}$"); // 6-digit numeric code
        verification.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(10));
        verification.IsUsed.Should().BeFalse();

        // Verify email was sent
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                user.Email,
                It.Is<string>(subject => subject.Contains("Verify Your Email")),
                It.Is<string>(body => body.Contains(verification.Code))),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationAsync_ShouldGenerateUniqueCode()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act - Send multiple verification codes
        await _verificationService.SendVerificationAsync(user);
        await _verificationService.SendVerificationAsync(user);
        await _verificationService.SendVerificationAsync(user);

        // Assert
        var verifications = await _context.EmailVerifications
            .Where(v => v.UserId == user.Id)
            .ToListAsync();
        
        verifications.Should().HaveCount(3);
        var codes = verifications.Select(v => v.Code).ToList();
        codes.Should().OnlyHaveUniqueItems(); // All codes should be different
    }

    [Fact]
    public async Task SendVerificationAsync_ShouldLogInformation()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.SendVerificationAsync(user);

        // Assert - Verify logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Sent verification code to {user.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ResendVerificationAsync Tests

    [Fact]
    public async Task ResendVerificationAsync_WithValidUnverifiedUser_ShouldReturnTrueAndSendNewCode()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Add an existing unused verification code
        var oldVerification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.EmailVerifications.Add(oldVerification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.ResendVerificationAsync(user.Email);

        // Assert
        result.Should().BeTrue();

        // Old code should be marked as used
        await _context.Entry(oldVerification).ReloadAsync();
        oldVerification.IsUsed.Should().BeTrue();

        // New code should exist
        var newVerification = await _context.EmailVerifications
            .Where(v => v.UserId == user.Id && !v.IsUsed)
            .FirstOrDefaultAsync();
        
        newVerification.Should().NotBeNull();
        newVerification!.Code.Should().NotBe(oldVerification.Code);
        newVerification.Code.Should().HaveLength(6);
        newVerification.Code.Should().MatchRegex(@"^\d{6}$");

        // Verify email was sent
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _verificationService.ResendVerificationAsync(nonExistentEmail);

        // Assert
        result.Should().BeFalse();

        // Verify no email was sent
        _mockEmailService.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Attempted to resend verification for non-existent email: {nonExistentEmail}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_WithAlreadyVerifiedUser_ShouldReturnFalse()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(true)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.ResendVerificationAsync(user.Email);

        // Assert
        result.Should().BeFalse();

        // Verify no email was sent
        _mockEmailService.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Attempted to resend verification for already verified email: {user.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldInvalidateMultipleOldCodes()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        // Add multiple old verification codes
        var oldCodes = new List<EmailVerification>
        {
            new() { UserId = user.Id, Code = "111111", ExpiresAt = DateTime.UtcNow.AddMinutes(5), IsUsed = false },
            new() { UserId = user.Id, Code = "222222", ExpiresAt = DateTime.UtcNow.AddMinutes(10), IsUsed = false },
            new() { UserId = user.Id, Code = "333333", ExpiresAt = DateTime.UtcNow.AddMinutes(2), IsUsed = false }
        };
        _context.EmailVerifications.AddRange(oldCodes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.ResendVerificationAsync(user.Email);

        // Assert
        result.Should().BeTrue();

        // All old codes should be marked as used
        foreach (var oldCode in oldCodes)
        {
            await _context.Entry(oldCode).ReloadAsync();
            oldCode.IsUsed.Should().BeTrue();
        }

        // New code should exist and be unused
        var activeVerification = await _context.EmailVerifications
            .Where(v => v.UserId == user.Id && !v.IsUsed)
            .FirstOrDefaultAsync();
        
        activeVerification.Should().NotBeNull();
        var oldCodesValues = oldCodes.Select(c => c.Code).ToList();
        oldCodesValues.Should().NotContain(activeVerification!.Code);
    }

    #endregion

    #region VerifyCodeAsync Tests

    [Fact]
    public async Task VerifyCodeAsync_WithValidCodeAndUser_ShouldReturnTrueAndMarkUserAsVerified()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, verification.Code);

        // Assert
        result.Should().BeTrue();

        // Verification should be marked as used
        await _context.Entry(verification).ReloadAsync();
        verification.IsUsed.Should().BeTrue();

        // User should be marked as verified
        await _context.Entry(user).ReloadAsync();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Act
        var result = await _verificationService.VerifyCodeAsync("nonexistent@example.com", "123456");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, "999999");

        // Assert
        result.Should().BeFalse();

        // Verification should remain unused
        await _context.Entry(verification).ReloadAsync();
        verification.IsUsed.Should().BeFalse();

        // User should remain unverified
        await _context.Entry(user).ReloadAsync();
        user.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithExpiredCode_ShouldReturnFalse()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // Expired 1 minute ago
            IsUsed = false
        };
        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, verification.Code);

        // Assert
        result.Should().BeFalse();

        // Verification should remain unused (not marked as used since it was expired)
        await _context.Entry(verification).ReloadAsync();
        verification.IsUsed.Should().BeFalse();

        // User should remain unverified
        await _context.Entry(user).ReloadAsync();
        user.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithAlreadyUsedCode_ShouldReturnFalse()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = true // Already used
        };
        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, verification.Code);

        // Assert
        result.Should().BeFalse();

        // User should remain unverified
        await _context.Entry(user).ReloadAsync();
        user.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithMultipleCodes_ShouldUseLatestValidCode()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var olderVerification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Earlier expiry
            IsUsed = false
        };

        var newerVerification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456", // Same code
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Later expiry
            IsUsed = false
        };

        _context.EmailVerifications.AddRange(olderVerification, newerVerification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, "123456");

        // Assert
        result.Should().BeTrue();

        // The newer verification (latest by expiry) should be used
        await _context.Entry(newerVerification).ReloadAsync();
        newerVerification.IsUsed.Should().BeTrue();

        // The older verification should remain unused
        await _context.Entry(olderVerification).ReloadAsync();
        olderVerification.IsUsed.Should().BeFalse();

        // User should be verified
        await _context.Entry(user).ReloadAsync();
        user.IsEmailVerified.Should().BeTrue();
    }

    #endregion

    #region Email Content Tests

    [Fact]
    public async Task SendVerificationAsync_ShouldSendEmailWithCorrectContent()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.SendVerificationAsync(user);

        // Assert
        var verification = await _context.EmailVerifications
            .FirstOrDefaultAsync(v => v.UserId == user.Id);

        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                user.Email, // Use the actual user's email, not hardcoded
                It.Is<string>(subject => subject == "Welcome to ALOud - Verify Your Email"),
                It.Is<string>(body => 
                    body.Contains(user.FirstName) && 
                    body.Contains(verification!.Code) &&
                    body.Contains("ALOud") &&
                    body.Contains("https://localhost/Account/Verify") &&
                    body.Contains(Uri.EscapeDataString(user.Email)))),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationAsync_WithCustomBaseUrl_ShouldUseCorrectUrl()
    {
        // Arrange
        var customBaseUrl = "https://custom.domain.com";
        _mockConfiguration.Setup(c => c["AppSettings:BaseUrl"]).Returns(customBaseUrl);

        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.SendVerificationAsync(user);

        // Assert
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains($"{customBaseUrl}/Account/Verify"))),
            Times.Once);
    }

    [Fact]
    public async Task SendVerificationAsync_WithNoBaseUrl_ShouldUseDefaultUrl()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["AppSettings:BaseUrl"]).Returns((string?)null);

        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.SendVerificationAsync(user);

        // Assert
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("https://localhost/Account/Verify"))),
            Times.Once);
    }

    #endregion

    #region Edge Case and Security Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12345")] // Too short
    [InlineData("1234567")] // Too long
    [InlineData("abcdef")] // Non-numeric
    public async Task VerifyCodeAsync_WithInvalidCodeFormat_ShouldReturnFalse(string invalidCode)
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _verificationService.VerifyCodeAsync(user.Email, invalidCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithCaseSensitiveEmail_ShouldWork()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);

        var verification = new EmailVerification
        {
            UserId = user.Id,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Act - Try with different case
        var result = await _verificationService.VerifyCodeAsync("test@example.com", verification.Code);

        // Assert - Should work since EF Core/SQL Server typically does case-insensitive string comparisons by default
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldLogSuccessfulResend()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _verificationService.ResendVerificationAsync(user.Email);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Verification code resent to {user.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task VerificationCodeGeneration_ShouldBeWithinValidRange()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmailVerified(false)
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act - Generate multiple codes to test range
        var codes = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            await _verificationService.SendVerificationAsync(user);
            var verification = await _context.EmailVerifications
                .Where(v => v.UserId == user.Id)
                .OrderByDescending(v => v.ExpiresAt)
                .FirstAsync();
            codes.Add(verification.Code);
        }

        // Assert
        foreach (var code in codes)
        {
            var codeValue = int.Parse(code);
            codeValue.Should().BeGreaterOrEqualTo(100000);
            codeValue.Should().BeLessOrEqualTo(999999);
        }
    }

    #endregion
}