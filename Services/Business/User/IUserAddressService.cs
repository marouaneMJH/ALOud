using ALOud.DTOs;
using ALOud.Models;

namespace ALOud.Services
{
    public interface IUserAddressService
    {
        // Address management for authenticated users
        Task<List<AddressDto>> GetUserAddressesAsync(Guid userId);
        Task<AddressDto?> GetUserAddressAsync(Guid userId, Guid addressId);
        Task<AddressDto?> GetDefaultAddressAsync(Guid userId);
        Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto dto);
        Task<AddressDto> UpdateAddressAsync(Guid userId, UpdateAddressDto dto);
        Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
        Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId);
        
        // Helper methods
        Task<bool> ValidateAddressOwnershipAsync(Guid userId, Guid addressId);
        Task<UserAddress?> GetUserAddressEntityAsync(Guid addressId);
        
        // Migration helpers for existing users
        Task MigrateUserAddressAsync(Guid userId, string legacyAddress);
    }
}