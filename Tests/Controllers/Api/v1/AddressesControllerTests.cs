using ALOud.Controllers.Api.v1;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ALOud.Tests.Controllers.Api.v1
{
    public class AddressesControllerTests
    {
        private readonly Mock<IUserAddressService> _mockAddressService;
        private readonly Mock<ILogger<AddressesController>> _mockLogger;
        private readonly AddressesController _controller;

        public AddressesControllerTests()
        {
            _mockAddressService = new Mock<IUserAddressService>();
            _mockLogger = new Mock<ILogger<AddressesController>>();
            _controller = new AddressesController(_mockAddressService.Object, _mockLogger.Object);
            
            // Setup authenticated user context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAddresses_ShouldReturnUserAddresses()
        {
            // Arrange
            var userId = GetUserId();
            var addresses = new List<AddressDto>
            {
                new AddressDto
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    AddressLine1 = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    PostalCode = "90210",
                    Country = "USA",
                    AddressType = "Home",
                    IsDefault = true
                }
            };

            _mockAddressService.Setup(s => s.GetUserAddressesAsync(userId))
                .ReturnsAsync(addresses);

            // Act
            var result = await _controller.GetAddresses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verify the service was called with correct userId
            _mockAddressService.Verify(s => s.GetUserAddressesAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateAddress_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var userId = GetUserId();
            var createDto = new CreateAddressDto
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

            var createdAddress = new AddressDto
            {
                Id = Guid.NewGuid(),
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                AddressLine1 = createDto.AddressLine1,
                City = createDto.City,
                State = createDto.State,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                AddressType = createDto.AddressType,
                IsDefault = createDto.IsDefault
            };

            _mockAddressService.Setup(s => s.CreateAddressAsync(userId, createDto))
                .ReturnsAsync(createdAddress);

            // Act
            var result = await _controller.CreateAddress(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
            Assert.Equal(nameof(AddressesController.GetAddress), createdResult.ActionName);
            
            // Verify the service was called
            _mockAddressService.Verify(s => s.CreateAddressAsync(userId, createDto), Times.Once);
        }

        [Fact]
        public async Task GetAddress_WithValidId_ShouldReturnAddress()
        {
            // Arrange
            var userId = GetUserId();
            var addressId = Guid.NewGuid();
            var address = new AddressDto
            {
                Id = addressId,
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

            _mockAddressService.Setup(s => s.ValidateAddressOwnershipAsync(userId, addressId))
                .ReturnsAsync(true);
            _mockAddressService.Setup(s => s.GetUserAddressAsync(userId, addressId))
                .ReturnsAsync(address);

            // Act
            var result = await _controller.GetAddress(addressId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verify the service was called
            _mockAddressService.Verify(s => s.ValidateAddressOwnershipAsync(userId, addressId), Times.Once);
            _mockAddressService.Verify(s => s.GetUserAddressAsync(userId, addressId), Times.Once);
        }

        [Fact]
        public async Task GetAddress_WithInvalidOwnership_ShouldReturnNotFound()
        {
            // Arrange
            var userId = GetUserId();
            var addressId = Guid.NewGuid();

            _mockAddressService.Setup(s => s.ValidateAddressOwnershipAsync(userId, addressId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetAddress(addressId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            
            // Verify only ownership validation was called
            _mockAddressService.Verify(s => s.ValidateAddressOwnershipAsync(userId, addressId), Times.Once);
            _mockAddressService.Verify(s => s.GetUserAddressAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAddress_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var userId = GetUserId();
            var addressId = Guid.NewGuid();

            _mockAddressService.Setup(s => s.ValidateAddressOwnershipAsync(userId, addressId))
                .ReturnsAsync(true);
            _mockAddressService.Setup(s => s.DeleteAddressAsync(userId, addressId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verify the service was called
            _mockAddressService.Verify(s => s.ValidateAddressOwnershipAsync(userId, addressId), Times.Once);
            _mockAddressService.Verify(s => s.DeleteAddressAsync(userId, addressId), Times.Once);
        }

        [Fact]
        public async Task SetDefaultAddress_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var userId = GetUserId();
            var addressId = Guid.NewGuid();

            _mockAddressService.Setup(s => s.ValidateAddressOwnershipAsync(userId, addressId))
                .ReturnsAsync(true);
            _mockAddressService.Setup(s => s.SetDefaultAddressAsync(userId, addressId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SetDefaultAddress(addressId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verify the service was called
            _mockAddressService.Verify(s => s.ValidateAddressOwnershipAsync(userId, addressId), Times.Once);
            _mockAddressService.Verify(s => s.SetDefaultAddressAsync(userId, addressId), Times.Once);
        }

        private Guid GetUserId()
        {
            var userIdClaim = _controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}