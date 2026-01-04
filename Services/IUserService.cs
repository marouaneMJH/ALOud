
using ALOud.DTOs;
using ALOud.Models;

namespace ALOud.Services;

// User-related operations: create, authenticate, and lookup.
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto dto);
    Task<User?> AuthenticateAsync(LoginDto dto);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
}
