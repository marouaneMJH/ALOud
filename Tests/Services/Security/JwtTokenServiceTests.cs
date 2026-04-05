using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ALOud.Services.Security;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.Services.Security
{
    public class JwtTokenServiceTests : IDisposable
    {
        private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _jwtSectionMock;
        private readonly JwtTokenService _jwtTokenService;

        // Test configuration values
        private const string TestSecretKey = "TestSecretKeyForJwtTokenGenerationMustBe32Characters123456789!";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";
        private const int TestExpirationMinutes = 30;

        public JwtTokenServiceTests()
        {
            _loggerMock = new Mock<ILogger<JwtTokenService>>();
            _configurationMock = new Mock<IConfiguration>();
            _jwtSectionMock = new Mock<IConfigurationSection>();

            // Setup configuration mock
            _configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns(TestSecretKey);
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);
            _configurationMock.Setup(x => x["Jwt:ExpirationMinutes"]).Returns(TestExpirationMinutes.ToString());

            _jwtTokenService = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            // Clean up any resources if needed
        }

        [Fact]
        public void GenerateToken_WithValidParameters_ShouldReturnValidToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "test@example.com";

            // Act
            var token = _jwtTokenService.GenerateToken(userId, email);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.CanReadToken(token).Should().BeTrue();
            
            var jwtToken = tokenHandler.ReadJwtToken(token);
            jwtToken.Issuer.Should().Be(TestIssuer);
            jwtToken.Audiences.Should().Contain(TestAudience);
            
            var claims = jwtToken.Claims.ToList();
            claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            claims.Should().NotContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void GenerateToken_WithAdminRole_ShouldIncludeAdminClaim()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "admin@example.com";

            // Act
            var token = _jwtTokenService.GenerateToken(userId, email, isAdmin: true);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.ToList();
            claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void GenerateToken_WithoutAdminRole_ShouldNotIncludeAdminClaim()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "user@example.com";

            // Act
            var token = _jwtTokenService.GenerateToken(userId, email, isAdmin: false);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.ToList();
            claims.Should().NotContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void GenerateToken_ShouldHaveCorrectExpiration()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "test@example.com";
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _jwtTokenService.GenerateToken(userId, email);
            var afterGeneration = DateTime.UtcNow;

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Token should expire within the configured time range (with some tolerance for timing)
            var expectedMinExpiry = beforeGeneration.AddMinutes(TestExpirationMinutes).AddSeconds(-1);
            var expectedMaxExpiry = afterGeneration.AddMinutes(TestExpirationMinutes).AddSeconds(1);
            
            jwtToken.ValidTo.Should().BeOnOrAfter(expectedMinExpiry);
            jwtToken.ValidTo.Should().BeOnOrBefore(expectedMaxExpiry);
        }

        [Theory]
        [InlineData("", "test@example.com")]
        [InlineData("user123", "")]
        public void GenerateToken_WithEmptyParameters_ShouldGenerateToken(string userId, string email)
        {
            // Note: The service accepts empty strings but not null values
            // This test verifies that empty strings don't throw exceptions
            
            // Act & Assert - Should not throw for empty strings
            var act = () => _jwtTokenService.GenerateToken(userId, email);
            act.Should().NotThrow();
            
            var token = _jwtTokenService.GenerateToken(userId, email);
            token.Should().NotBeNullOrEmpty();
            
            // Verify the token contains the empty values
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            
            // Claims should exist with empty values
            userIdClaim.Should().NotBeNull();
            emailClaim.Should().NotBeNull();
            userIdClaim!.Value.Should().Be(userId);
            emailClaim!.Value.Should().Be(email);
        }

        [Theory]
        [InlineData(null, "test@example.com")]
        [InlineData("user123", null)]
        public void GenerateToken_WithNullParameters_ShouldThrowException(string userId, string email)
        {
            // The JwtTokenService throws ArgumentNullException for null parameters
            // This is the correct security behavior for authentication services
            
            // Act & Assert - Should throw for null values
            var act = () => _jwtTokenService.GenerateToken(userId, email);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "test@example.com";
            var token = _jwtTokenService.GenerateToken(userId, email);

            // Act
            var principal = _jwtTokenService.ValidateToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email);
        }

        [Fact]
        public void ValidateToken_WithValidAdminToken_ShouldReturnPrincipalWithAdminRole()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var email = "admin@example.com";
            var token = _jwtTokenService.GenerateToken(userId, email, isAdmin: true);

            // Act
            var principal = _jwtTokenService.ValidateToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal.IsInRole("Admin").Should().BeTrue();
            principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid.token.here")]
        [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
        [InlineData(null)]
        public void ValidateToken_WithInvalidToken_ShouldReturnNull(string invalidToken)
        {
            // Act
            var principal = _jwtTokenService.ValidateToken(invalidToken);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithTokenFromDifferentService_ShouldReturnNull()
        {
            // Arrange - Create service with different secret
            var differentConfigMock = new Mock<IConfiguration>();
            differentConfigMock.Setup(x => x["Jwt:SecretKey"]).Returns("DifferentSecretKey123456789DifferentSecretKey123456789!");
            differentConfigMock.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            differentConfigMock.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);
            differentConfigMock.Setup(x => x["Jwt:ExpirationMinutes"]).Returns(TestExpirationMinutes.ToString());

            var differentService = new JwtTokenService(differentConfigMock.Object, _loggerMock.Object);
            var tokenFromDifferentService = differentService.GenerateToken("user123", "test@example.com");

            // Act
            var principal = _jwtTokenService.ValidateToken(tokenFromDifferentService);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMissingConfiguration_ShouldUseDefaults()
        {
            // Arrange
            var emptyConfigMock = new Mock<IConfiguration>();
            emptyConfigMock.Setup(x => x[It.IsAny<string>()]).Returns((string)null);

            // Act & Assert - Should not throw
            var service = new JwtTokenService(emptyConfigMock.Object, _loggerMock.Object);
            
            // Should be able to generate token with default values
            var token = service.GenerateToken("test123", "test@example.com");
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Constructor_WithEnvironmentVariables_ShouldUseEnvironmentValues()
        {
            // Arrange
            var envSecretKey = "EnvSecretKey123456789EnvSecretKey123456789!";
            var envIssuer = "EnvIssuer";
            var envAudience = "EnvAudience";
            
            // Mock configuration to return null (simulating missing appsettings)
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x[It.IsAny<string>()]).Returns((string)null);
            
            // Set environment variables
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", envSecretKey);
            Environment.SetEnvironmentVariable("JWT_ISSUER", envIssuer);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", envAudience);
            Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "90");

            try
            {
                // Act
                var service = new JwtTokenService(configMock.Object, _loggerMock.Object);
                var token = service.GenerateToken("test123", "test@example.com");

                // Assert
                token.Should().NotBeNullOrEmpty();
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                jwtToken.Issuer.Should().Be(envIssuer);
                jwtToken.Audiences.Should().Contain(envAudience);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
                Environment.SetEnvironmentVariable("JWT_ISSUER", null);
                Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
                Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", null);
            }
        }

        [Fact]
        public void Constructor_WithInvalidExpirationMinutes_ShouldUseDefault()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["Jwt:SecretKey"]).Returns(TestSecretKey);
            configMock.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            configMock.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);
            configMock.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("invalid_number");

            // Act & Assert - Should not throw
            var service = new JwtTokenService(configMock.Object, _loggerMock.Object);
            
            var token = service.GenerateToken("test123", "test@example.com");
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Should use default 60 minutes
            var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
            jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void GenerateToken_ShouldLogInformation()
        {
            // Arrange
            var userId = "test123";
            var email = "test@example.com";

            // Act
            var token = _jwtTokenService.GenerateToken(userId, email);

            // Assert
            token.Should().NotBeNullOrEmpty();
            // Verify that constructor logged configuration info
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JWT configured")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldLogWarning()
        {
            // Arrange
            var invalidToken = "invalid.jwt.token";

            // Act
            var result = _jwtTokenService.ValidateToken(invalidToken);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid JWT token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TokenGeneration_ShouldCreateUniqueTokensForDifferentUsers()
        {
            // Arrange
            var userId1 = "user1";
            var email1 = "user1@example.com";
            var userId2 = "user2";
            var email2 = "user2@example.com";

            // Act
            var token1 = _jwtTokenService.GenerateToken(userId1, email1);
            var token2 = _jwtTokenService.GenerateToken(userId2, email2);

            // Assert
            token1.Should().NotBe(token2);
            
            var principal1 = _jwtTokenService.ValidateToken(token1);
            var principal2 = _jwtTokenService.ValidateToken(token2);
            
            principal1.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId1);
            principal2.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId2);
            principal1.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email1);
            principal2.FindFirst(ClaimTypes.Email)?.Value.Should().Be(email2);
        }
    }
}