using FluentAssertions;
using ALOud.Services.Security;
using Xunit;

namespace Tests.Services.Security;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _passwordHasherService;

    public PasswordHasherServiceTests()
    {
        _passwordHasherService = new PasswordHasherService();
    }

    #region Hash Method Tests

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hashedPassword = _passwordHasherService.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(50); // ASP.NET Core Identity hashes are typically longer
    }

    [Fact]
    public void Hash_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hash1 = _passwordHasherService.Hash(password);
        var hash2 = _passwordHasherService.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("ComplexP@ssw0rd!")]
    [InlineData("123456789")]
    [InlineData("P@$$w0rd")]
    [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%^&*()_+")]
    public void Hash_WithVariousPasswords_ShouldReturnValidHashes(string password)
    {
        // Act
        var hashedPassword = _passwordHasherService.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void Hash_WithEmptyString_ShouldReturnHash()
    {
        // Arrange
        var password = "";

        // Act
        var hashedPassword = _passwordHasherService.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void Hash_WithNullPassword_ShouldThrowException()
    {
        // Arrange
        string password = null!;

        // Act & Assert
        var act = () => _passwordHasherService.Hash(password);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Hash_WithUnicodeCharacters_ShouldReturnValidHash()
    {
        // Arrange
        var password = "Pässwörd123🔒";

        // Act
        var hashedPassword = _passwordHasherService.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    #endregion

    #region Verify Method Tests

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword123";
        var hashedPassword = _passwordHasherService.Hash(password);

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123";
        var wrongPassword = "WrongPassword";
        var hashedPassword = _passwordHasherService.Hash(password);

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyProvidedPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123";
        var hashedPassword = _passwordHasherService.Hash(password);

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyHashAndEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var hashedPassword = _passwordHasherService.Hash("");

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, "");

        // Assert
        result.Should().BeTrue(); // Empty password should match empty password hash
    }

    [Fact]
    public void Verify_WithNullHashedPassword_ShouldThrowException()
    {
        // Arrange
        string hashedPassword = null!;
        var providedPassword = "password";

        // Act & Assert
        var act = () => _passwordHasherService.Verify(hashedPassword, providedPassword);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_WithNullProvidedPassword_ShouldThrowException()
    {
        // Arrange
        var password = "MySecurePassword123";
        var hashedPassword = _passwordHasherService.Hash(password);
        string providedPassword = null!;

        // Act & Assert
        var act = () => _passwordHasherService.Verify(hashedPassword, providedPassword);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ShouldThrowException()
    {
        // Arrange
        var invalidHash = "NotAValidHash";
        var password = "MySecurePassword123";

        // Act & Assert - Invalid base64 hash format throws FormatException
        var act = () => _passwordHasherService.Verify(invalidHash, password);
        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("ComplexP@ssw0rd!")]
    [InlineData("123456789")]
    [InlineData("P@$$w0rd")]
    [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%^&*()_+")]
    public void Verify_WithVariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Arrange
        var hashedPassword = _passwordHasherService.Hash(password);

        // Act
        var correctResult = _passwordHasherService.Verify(hashedPassword, password);
        var incorrectResult = _passwordHasherService.Verify(hashedPassword, password + "Wrong");

        // Assert
        correctResult.Should().BeTrue();
        incorrectResult.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "Pässwörd123🔒";
        var hashedPassword = _passwordHasherService.Hash(password);

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "MySecurePassword123";
        var hashedPassword = _passwordHasherService.Hash(password);
        var wrongCasePassword = "mysecurepassword123";

        // Act
        var result = _passwordHasherService.Verify(hashedPassword, wrongCasePassword);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Security and Edge Case Tests

    [Fact]
    public void Hash_And_Verify_WithVeryLongPassword_ShouldWorkCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000); // 1000 character password

        // Act
        var hashedPassword = _passwordHasherService.Hash(longPassword);
        var result = _passwordHasherService.Verify(hashedPassword, longPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        result.Should().BeTrue();
    }

    [Fact]
    public void Hash_And_Verify_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "!@#$%^&*()_+-={}[]|\\:;\"'<>,.?/~`";

        // Act
        var hashedPassword = _passwordHasherService.Hash(password);
        var result = _passwordHasherService.Verify(hashedPassword, password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        result.Should().BeTrue();
    }

    [Fact]
    public void Hash_And_Verify_WithWhitespacePassword_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "  password with spaces  ";

        // Act
        var hashedPassword = _passwordHasherService.Hash(password);
        var result = _passwordHasherService.Verify(hashedPassword, password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        result.Should().BeTrue();
        
        // Should not match trimmed password
        var trimmedResult = _passwordHasherService.Verify(hashedPassword, password.Trim());
        trimmedResult.Should().BeFalse();
    }

    [Fact]
    public void Verify_CrossHashCheck_DifferentHashesShouldNotMatch()
    {
        // Arrange
        var password1 = "password1";
        var password2 = "password2";
        var hash1 = _passwordHasherService.Hash(password1);
        var hash2 = _passwordHasherService.Hash(password2);

        // Act & Assert
        _passwordHasherService.Verify(hash1, password1).Should().BeTrue();
        _passwordHasherService.Verify(hash2, password2).Should().BeTrue();
        _passwordHasherService.Verify(hash1, password2).Should().BeFalse();
        _passwordHasherService.Verify(hash2, password1).Should().BeFalse();
    }

    [Fact]
    public void Hash_MultipleCallsInParallel_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123";
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _passwordHasherService.Hash(password)));
        }

        var hashes = Task.WhenAll(tasks).Result;

        // Assert
        hashes.Should().HaveCount(10);
        hashes.Should().OnlyContain(hash => !string.IsNullOrEmpty(hash));
        hashes.Distinct().Should().HaveCount(10); // All hashes should be unique
        
        // All hashes should verify correctly
        foreach (var hash in hashes)
        {
            _passwordHasherService.Verify(hash, password).Should().BeTrue();
        }
    }

    #endregion
}