using System;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ALOud.Services;
using ALOud.Services.Security;
using ALOud.Tests.Helpers;
using Tests.Common.TestDataBuilders;
using ALOud.DTOs;
using ALOud.Models;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for UserService using Strategy B (EF InMemory).
    /// Tests the UserService business logic with in-memory database and mocked external dependencies.
    /// 
    /// Test naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class UserServiceTests : IDisposable
    {
        private readonly UserService _userService;
        private readonly Data.ALOudDbContext _context;
        private readonly PasswordHasherService _passwordHasher;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<UserService>> _mockLogger;

        public UserServiceTests()
        {
            // Arrange - Create in-memory database context
            _context = DbContextFactory.CreateAndEnsureCreated();
            
            // Create real PasswordHasherService (it's pure logic, no I/O)
            _passwordHasher = new PasswordHasherService();
            
            // Mock external dependencies
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<UserService>>();
            
            // Create service under test
            _userService = new UserService(
                _context,
                _passwordHasher,
                _mockEmailService.Object,
                _mockLogger.Object);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_WhenEmailDoesNotExist_ReturnsUserWithCorrectFieldsAndSavesToDb()
        {
            // Arrange
            var createUserDto = new CreateUserDtoBuilder()
                .WithEmail("new.user@test.com")
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithAddress("123 Test Street, Test City")
                .Build();

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("new.user@test.com");
            result.Address.Should().Be("123 Test Street, Test City");
            result.IsActive.Should().BeTrue();
            result.IsEmailVerified.Should().BeFalse(); // Default value
            result.PasswordHash.Should().NotBeNullOrEmpty();
            
            // Verify password hash is correct
            _passwordHasher.Verify(result.PasswordHash, createUserDto.Password).Should().BeTrue();

            // Verify user was saved to database
            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("new.user@test.com");
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailDoesNotExist_SendsWelcomeEmail()
        {
            // Arrange
            var createUserDto = new CreateUserDtoBuilder()
                .WithEmail("welcome@test.com")
                .WithFirstName("Jane")
                .Build();

            // Act
            await _userService.CreateUserAsync(createUserDto);

            // Assert
            _mockEmailService.Verify(
                x => x.SendEmailAsync(
                    "welcome@test.com",
                    "Welcome to ALOud",
                    It.Is<string>(body => body.Contains("Jane") && body.Contains("Welcome to ALOud"))),
                Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingEmail = "existing@test.com";
            var existingUser = new UserBuilder()
                .WithEmail(existingEmail)
                .Build();
            
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = new CreateUserDtoBuilder()
                .WithEmail(existingEmail)
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.CreateUserAsync(createUserDto));
            
            exception.Message.Should().Be("Email already exists");
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailServiceThrows_DoesNotRethrowAndUserStillCreated()
        {
            // Arrange
            var createUserDto = new CreateUserDtoBuilder()
                .WithEmail("email.fail@test.com")
                .Build();

            _mockEmailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("email.fail@test.com");
            
            // Verify user was still saved despite email failure
            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUserAsync_WhenDbUpdateExceptionOnSave_ThrowsInvalidOperationExceptionWithDatabaseError()
        {
            // Arrange
            var createUserDto = new CreateUserDtoBuilder().Build();
            
            // Dispose the original context to simulate database error
            _context.Dispose();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.CreateUserAsync(createUserDto));
            
            exception.Message.Should().Be("Failed to create user: database error");
            exception.InnerException.Should().BeOfType<ObjectDisposedException>();
        }

        #endregion

        #region AuthenticateAsync Tests

        [Fact]
        public async Task AuthenticateAsync_WhenUserExistsEmailVerifiedCorrectPassword_ReturnsUser()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = _passwordHasher.Hash(password);
            
            var user = new UserBuilder()
                .WithEmail("auth@test.com")
                .WithPasswordHash(hashedPassword)
                .WithEmailVerified(true)
                .WithActiveStatus(true)
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDtoBuilder()
                .WithCredentials("auth@test.com", password)
                .Build();

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Email.Should().Be("auth@test.com");
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserExistsEmailVerifiedWrongPassword_ReturnsNull()
        {
            // Arrange
            var correctPassword = "TestPassword123!";
            var wrongPassword = "WrongPassword456!";
            var hashedPassword = _passwordHasher.Hash(correctPassword);
            
            var user = new UserBuilder()
                .WithEmail("wrong.password@test.com")
                .WithPasswordHash(hashedPassword)
                .WithEmailVerified(true)
                .WithActiveStatus(true)
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDtoBuilder()
                .WithCredentials("wrong.password@test.com", wrongPassword)
                .Build();

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserExistsButEmailNotVerified_ReturnsNull()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = _passwordHasher.Hash(password);
            
            var user = new UserBuilder()
                .WithEmail("unverified@test.com")
                .WithPasswordHash(hashedPassword)
                .WithEmailVerified(false) // Email not verified
                .WithActiveStatus(true)
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDtoBuilder()
                .WithCredentials("unverified@test.com", password)
                .Build();

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserExistsButIsNotActive_ReturnsNull()
        {
            // Arrange
            var password = "TestPassword123!";
            var hashedPassword = _passwordHasher.Hash(password);
            
            var user = new UserBuilder()
                .WithEmail("inactive@test.com")
                .WithPasswordHash(hashedPassword)
                .WithEmailVerified(true)
                .WithActiveStatus(false) // User is inactive
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDtoBuilder()
                .WithCredentials("inactive@test.com", password)
                .Build();

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WhenEmailNotFound_ReturnsNull()
        {
            // Arrange
            var loginDto = new LoginDtoBuilder()
                .WithCredentials("notfound@test.com", "password")
                .Build();

            // Act
            var result = await _userService.AuthenticateAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenUserExistsAndIsActive_ReturnsUser()
        {
            // Arrange
            var user = new UserBuilder()
                .WithActiveStatus(true)
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExistsButIsNotActive_ReturnsNull()
        {
            // Arrange
            var user = new UserBuilder()
                .WithActiveStatus(false) // User is inactive
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByIdAsync(user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userService.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByEmailAsync Tests

        [Fact]
        public async Task GetByEmailAsync_WhenEmailExists_ReturnsUser()
        {
            // Arrange
            var user = new UserBuilder()
                .WithEmail("exists@test.com")
                .Build();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByEmailAsync("exists@test.com");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Email.Should().Be("exists@test.com");
        }

        [Fact]
        public async Task GetByEmailAsync_WhenEmailDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _userService.GetByEmailAsync("notexists@test.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsUserRegardlessOfActiveStatus()
        {
            // Arrange - Create an inactive user
            var inactiveUser = new UserBuilder()
                .WithEmail("inactive.user@test.com")
                .WithActiveStatus(false)
                .Build();

            _context.Users.Add(inactiveUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetByEmailAsync("inactive.user@test.com");

            // Assert - Should still return the user even if inactive
            result.Should().NotBeNull();
            result!.Id.Should().Be(inactiveUser.Id);
            result.IsActive.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateUserThenAuthenticate_FullWorkflow_Success()
        {
            // Arrange
            var createUserDto = new CreateUserDtoBuilder()
                .WithEmail("workflow@test.com")
                .WithPassword("TestPassword123!")
                .Build();

            // Act - Create user
            var createdUser = await _userService.CreateUserAsync(createUserDto);
            
            // Manually verify email (since we don't have verification service)
            createdUser.IsEmailVerified = true;
            _context.Users.Update(createdUser);
            await _context.SaveChangesAsync();

            // Act - Authenticate user
            var loginDto = new LoginDtoBuilder()
                .WithCredentials("workflow@test.com", "TestPassword123!")
                .Build();
            
            var authenticatedUser = await _userService.AuthenticateAsync(loginDto);

            // Assert
            authenticatedUser.Should().NotBeNull();
            authenticatedUser!.Id.Should().Be(createdUser.Id);
            authenticatedUser.Email.Should().Be("workflow@test.com");
        }

        #endregion
    }
}