using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using ALOud.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ALOud.Tests.Services.Business
{
    public class UserAddressServiceTests
    {
        private ALOudDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ALOudDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ALOudDbContext(options);
        }

        private UserAddressService GetService(ALOudDbContext context)
        {
            var logger = new Mock<ILogger<UserAddressService>>();
            return new UserAddressService(context, logger.Object);
        }

        [Fact]
        public async Task CreateAddressAsync_ShouldCreateNewAddress()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = GetService(context);
            
            var userId = Guid.NewGuid();
            var dto = new CreateAddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "90210",
                Country = "USA",
                AddressType = "Home",
                IsDefault = true
            };

            // Act
            var result = await service.CreateAddressAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.FirstName, result.FirstName);
            Assert.Equal(dto.LastName, result.LastName);
            Assert.Equal(dto.AddressLine1, result.AddressLine1);
            Assert.True(result.IsDefault);
            
            // Verify it was saved to database
            var savedAddress = await context.UserAddresses.FirstOrDefaultAsync(a => a.Id == result.Id);
            Assert.NotNull(savedAddress);
            Assert.Equal(userId, savedAddress.UserId);
        }

        [Fact]
        public async Task GetUserAddressesAsync_ShouldReturnUserAddresses()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = GetService(context);
            
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            // Add addresses for the user
            var address1 = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "90210",
                Country = "USA",
                AddressType = "Home",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var address2 = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "456 Work Ave",
                City = "Worktown",
                State = "CA",
                PostalCode = "90211",
                Country = "USA",
                AddressType = "Work",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add address for different user (should not be returned)
            var otherAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                FirstName = "Jane",
                LastName = "Smith",
                AddressLine1 = "789 Other St",
                City = "Otherville",
                State = "NY",
                PostalCode = "10001",
                Country = "USA",
                AddressType = "Home",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.UserAddresses.AddRange(address1, address2, otherAddress);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetUserAddressesAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, addr => Assert.Equal("John", addr.FirstName));
            
            // Default address should be first
            Assert.True(result.First().IsDefault);
            Assert.Equal("Home", result.First().AddressType);
        }

        [Fact]
        public async Task GetDefaultAddressAsync_ShouldReturnDefaultAddress()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = GetService(context);
            
            var userId = Guid.NewGuid();

            var defaultAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "90210",
                Country = "USA",
                AddressType = "Home",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var nonDefaultAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "456 Work Ave",
                City = "Worktown",
                State = "CA",
                PostalCode = "90211",
                Country = "USA",
                AddressType = "Work",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.UserAddresses.AddRange(defaultAddress, nonDefaultAddress);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetDefaultAddressAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDefault);
            Assert.Equal("Home", result.AddressType);
            Assert.Equal(defaultAddress.Id, result.Id);
        }

        [Fact]
        public async Task ValidateAddressOwnershipAsync_ShouldReturnCorrectOwnership()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var service = GetService(context);
            
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var userAddress = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "123 Main St",
                City = "Anytown",
                State = "CA",
                PostalCode = "90210",
                Country = "USA",
                AddressType = "Home",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.UserAddresses.Add(userAddress);
            await context.SaveChangesAsync();

            // Act & Assert
            var validOwnership = await service.ValidateAddressOwnershipAsync(userId, userAddress.Id);
            Assert.True(validOwnership);

            var invalidOwnership = await service.ValidateAddressOwnershipAsync(otherUserId, userAddress.Id);
            Assert.False(invalidOwnership);
        }
    }
}