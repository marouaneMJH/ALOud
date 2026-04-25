using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly ALOudDbContext _context;
        private readonly ILogger<UserAddressService> _logger;

        public UserAddressService(ALOudDbContext context, ILogger<UserAddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId)
        {
            _logger.LogInformation("Fetching addresses for user: {UserId}", userId);

            try
            {
                var addresses = await _context.UserAddresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.UpdatedAt)
                    .ToListAsync();

                return addresses.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<AddressDto?> GetUserAddressAsync(Guid userId, Guid addressId)
        {
            _logger.LogInformation("Fetching address {AddressId} for user: {UserId}", addressId, userId);

            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                return address != null ? MapToDto(address) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address {AddressId} for user: {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<AddressDto?> GetDefaultAddressAsync(Guid userId)
        {
            _logger.LogInformation("Fetching default address for user: {UserId}", userId);

            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                return address != null ? MapToDto(address) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching default address for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto dto)
        {
            _logger.LogInformation("Creating new address for user: {UserId}", userId);

            try
            {
                // If this is set as default, unset current default
                if (dto.IsDefault)
                {
                    await UnsetCurrentDefaultAsync(userId);
                }

                var address = new UserAddress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    Country = dto.Country,
                    PhoneNumber = dto.PhoneNumber,
                    AddressType = dto.AddressType,
                    IsDefault = dto.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserAddresses.Add(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Address created successfully: {AddressId}", address.Id);
                return MapToDto(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid userId, UpdateAddressDto dto)
        {
            _logger.LogInformation("Updating address {AddressId} for user: {UserId}", dto.Id, userId);

            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == dto.Id && a.UserId == userId);

                if (address == null)
                {
                    throw new ArgumentException($"Address {dto.Id} not found for user {userId}");
                }

                // If this is being set as default, unset current default
                if (dto.IsDefault && !address.IsDefault)
                {
                    await UnsetCurrentDefaultAsync(userId);
                }

                // Update fields
                address.FirstName = dto.FirstName;
                address.LastName = dto.LastName;
                address.AddressLine1 = dto.AddressLine1;
                address.AddressLine2 = dto.AddressLine2;
                address.City = dto.City;
                address.State = dto.State;
                address.PostalCode = dto.PostalCode;
                address.Country = dto.Country;
                address.PhoneNumber = dto.PhoneNumber;
                address.AddressType = dto.AddressType;
                address.IsDefault = dto.IsDefault;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Address updated successfully: {AddressId}", address.Id);
                return MapToDto(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user: {UserId}", dto.Id, userId);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
        {
            _logger.LogInformation("Deleting address {AddressId} for user: {UserId}", addressId, userId);

            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    _logger.LogWarning("Address {AddressId} not found for user: {UserId}", addressId, userId);
                    return false;
                }

                _context.UserAddresses.Remove(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Address deleted successfully: {AddressId}", addressId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user: {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<bool> SetDefaultAddressAsync(Guid userId, Guid addressId)
        {
            _logger.LogInformation("Setting default address {AddressId} for user: {UserId}", addressId, userId);

            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    _logger.LogWarning("Address {AddressId} not found for user: {UserId}", addressId, userId);
                    return false;
                }

                // Unset current default
                await UnsetCurrentDefaultAsync(userId);

                // Set new default
                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default address set successfully: {AddressId}", addressId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user: {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<bool> ValidateAddressOwnershipAsync(Guid userId, Guid addressId)
        {
            try
            {
                return await _context.UserAddresses
                    .AnyAsync(a => a.Id == addressId && a.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating address ownership for address {AddressId} and user: {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<UserAddress?> GetUserAddressEntityAsync(Guid addressId)
        {
            try
            {
                return await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address entity: {AddressId}", addressId);
                throw;
            }
        }

        public async Task MigrateUserAddressAsync(Guid userId, string legacyAddress)
        {
            _logger.LogInformation("Migrating legacy address for user: {UserId}", userId);

            try
            {
                // Check if user already has addresses
                var existingAddresses = await _context.UserAddresses
                    .Where(a => a.UserId == userId)
                    .CountAsync();

                if (existingAddresses > 0)
                {
                    _logger.LogInformation("User {UserId} already has addresses, skipping migration", userId);
                    return;
                }

                // Parse legacy address (simple approach - in real implementation, use address parsing service)
                var addressParts = legacyAddress.Split(',');
                
                var address = new UserAddress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = "Legacy",
                    LastName = "Address",
                    AddressLine1 = addressParts.Length > 0 ? addressParts[0].Trim() : legacyAddress,
                    City = addressParts.Length > 1 ? addressParts[1].Trim() : "Unknown",
                    State = addressParts.Length > 2 ? addressParts[2].Trim() : "Unknown",
                    PostalCode = addressParts.Length > 3 ? addressParts[3].Trim() : "00000",
                    Country = "United States", // Default assumption
                    AddressType = "Home",
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserAddresses.Add(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Legacy address migrated for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating legacy address for user: {UserId}", userId);
                throw;
            }
        }

        private async Task UnsetCurrentDefaultAsync(Guid userId)
        {
            var currentDefault = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                currentDefault.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static AddressDto MapToDto(UserAddress address)
        {
            return new AddressDto
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                PhoneNumber = address.PhoneNumber,
                AddressType = address.AddressType,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };
        }
    }
}