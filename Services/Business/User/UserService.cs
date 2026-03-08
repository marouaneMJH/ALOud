using System;
using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using ALOud.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services;

public class UserService : IUserService
{
    // User service: create users, authenticate, and lookup helper methods.
    private readonly ALOudDbContext _db;
    private readonly PasswordHasherService _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ALOudDbContext db,
        PasswordHasherService passwordHasher,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        _logger.LogInformation($"=== CREATE USER START === Email: {dto.Email}");

        try
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogWarning($"Email already exists: {dto.Email}");
                throw new InvalidOperationException("Email already exists");
            }

            _logger.LogInformation("Creating new user...");
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Address = dto.Address,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                IsActive = true
            };

            _logger.LogInformation($"User created with ID: {user.Id}, IsActive: {user.IsActive}");

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User successfully saved to database");

            await SendWelcomeEmailAsync(user);

            return user;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during user creation");
            throw new InvalidOperationException("Failed to create user: database error", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user creation");
            throw;
        }
    }

    public async Task<User?> AuthenticateAsync(LoginDto dto)
    {
        _logger.LogInformation($"=== AUTHENTICATION START === Email: {dto.Email}");

        try
        {
            _logger.LogInformation("Searching for user in database...");
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive && u.IsEmailVerified);

            if (user == null)
            {
                _logger.LogWarning($"User not found or inactive for email: {dto.Email}");

                // Check if user exists but is inactive
                var inactiveUser = await _db.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (inactiveUser != null)
                {
                    _logger.LogWarning($"User exists but IsActive = {inactiveUser.IsActive}");
                }
                else
                {
                    _logger.LogWarning("No user found with this email address");
                }

                return null;
            }

            _logger.LogInformation($"User found: ID={user.Id}, IsActive={user.IsActive}");
            _logger.LogInformation("Verifying password...");

            var isValid = _passwordHasher.Verify(
                user.PasswordHash,
                dto.Password
            );

            _logger.LogInformation($"Password verification result: {isValid}");

            return isValid ? user : null;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during authentication for email: {Email}", dto.Email);
            throw new InvalidOperationException("Authentication failed: database error", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for email: {Email}", dto.Email);
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error fetching user by id: {UserId}", id);
            throw new InvalidOperationException("Failed to fetch user: database error", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user by id: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error fetching user by email: {Email}", email);
            throw new InvalidOperationException("Failed to fetch user: database error", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user by email: {Email}", email);
            throw;
        }
    }

    private async Task SendWelcomeEmailAsync(User user)
    {
        var subject = "Welcome to ALOud";
        var body = $"<p>Hello {user.FirstName},</p><p>Welcome to ALOud! Your account has been created successfully.</p>";

        try
        {
            await _emailService.SendEmailAsync(user.Email, subject, body);
            _logger.LogInformation("Welcome email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
        }
    }

}
