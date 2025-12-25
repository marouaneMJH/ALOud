using System;
using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using ALOud.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace ALOud.Services;

public class UserService : IUserService
{
    private readonly ALOudDbContext _db;
    private readonly PasswordHasherService _passwordHasher;

    public UserService(
        ALOudDbContext db,
        PasswordHasherService passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // TODO Send Activation Email
        // TODO Send Welcome Email

        return user;
    }

    public async Task<User?> AuthenticateAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null)
            return null;

        var isValid = _passwordHasher.Verify(
            user.PasswordHash,
            dto.Password
        );

        return isValid ? user : null;
    }

}
