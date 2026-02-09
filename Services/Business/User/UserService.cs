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
    private readonly ILogger<UserService> _logger;

    public UserService(
        ALOudDbContext db,
        PasswordHasherService passwordHasher,
        ILogger<UserService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
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

            // TODO Send Activation Email
            // TODO Send Welcome Email

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during user creation");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during authentication");
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching user by id");
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching user by email");
            throw;
        }
    }

}
