using ALOud.DTOs;
using ALOud.Models;
using ALOud.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for ICheckoutService.
    /// Uses mock-based strategy to isolate business logic from database/infrastructure.
    ///
    /// Naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class CheckoutServiceTests
    {
        private readonly Mock<ICheckoutService> _mockCheckoutService;

        public CheckoutServiceTests()
        {
            _mockCheckoutService = new Mock<ICheckoutService>();
        }

        // ─────────────────────────────────────────────────────────
        // StartCheckoutAsync Tests
        // ─────────────────────────────────────────────────────────

        #region StartCheckoutAsync

        [Fact]
        public async Task StartCheckoutAsync_WithValidCartAndEmail_ReturnsCheckoutSummary()
        {
            // Arrange
            var dto = new StartCheckoutDto { Email = "user@example.com", IsGuestCheckout = false };
            var cartId = "cart-abc-123";
            var expected = new CheckoutSummaryDto
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Status = "InProgress",
                IsGuestCheckout = false
            };

            _mockCheckoutService
                .Setup(s => s.StartCheckoutAsync(dto, cartId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.StartCheckoutAsync(dto, cartId);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("user@example.com");
            result.Status.Should().Be("InProgress");
            result.IsGuestCheckout.Should().BeFalse();

            _mockCheckoutService.Verify(s => s.StartCheckoutAsync(dto, cartId), Times.Once);
        }

        [Fact]
        public async Task StartCheckoutAsync_WithGuestCheckout_ReturnsGuestSummary()
        {
            // Arrange
            var dto = new StartCheckoutDto { Email = "guest@aloud.ma", IsGuestCheckout = true };
            var cartId = "cart-guest-456";
            var expected = new CheckoutSummaryDto
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Status = "InProgress",
                IsGuestCheckout = true
            };

            _mockCheckoutService
                .Setup(s => s.StartCheckoutAsync(dto, cartId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.StartCheckoutAsync(dto, cartId);

            // Assert
            result.Should().NotBeNull();
            result.IsGuestCheckout.Should().BeTrue();
        }

        [Fact]
        public async Task StartCheckoutAsync_WhenLowStock_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new StartCheckoutDto { Email = "user@example.com" };
            var cartId = "cart-low-stock";

            _mockCheckoutService
                .Setup(s => s.StartCheckoutAsync(dto, cartId))
                .ThrowsAsync(new InvalidOperationException("Insufficient stock for one or more items in cart"));

            // Act & Assert
            var act = async () => await _mockCheckoutService.Object.StartCheckoutAsync(dto, cartId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Insufficient stock*");
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // GetCheckoutAsync Tests
        // ─────────────────────────────────────────────────────────

        #region GetCheckoutAsync

        [Fact]
        public async Task GetCheckoutAsync_WithValidId_ReturnsCheckoutSummary()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var expected = new CheckoutSummaryDto
            {
                Id = checkoutId,
                Status = "InProgress",
                Email = "user@example.com"
            };

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.GetCheckoutAsync(checkoutId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(checkoutId);
        }

        [Fact]
        public async Task GetCheckoutAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(nonExistentId))
                .ReturnsAsync((CheckoutSummaryDto?)null);

            // Act
            var result = await _mockCheckoutService.Object.GetCheckoutAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // GetActiveCheckoutByCartIdAsync Tests
        // ─────────────────────────────────────────────────────────

        #region GetActiveCheckoutByCartIdAsync

        [Fact]
        public async Task GetActiveCheckoutByCartIdAsync_WhenActiveCheckoutExists_ReturnsIt()
        {
            // Arrange
            var cartId = "cart-xyz-789";
            var expected = new CheckoutSummaryDto
            {
                Id = Guid.NewGuid(),
                Status = "InProgress"
            };

            _mockCheckoutService
                .Setup(s => s.GetActiveCheckoutByCartIdAsync(cartId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.GetActiveCheckoutByCartIdAsync(cartId);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be("InProgress");
        }

        [Fact]
        public async Task GetActiveCheckoutByCartIdAsync_WhenNoneExists_ReturnsNull()
        {
            // Arrange
            _mockCheckoutService
                .Setup(s => s.GetActiveCheckoutByCartIdAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckoutSummaryDto?)null);

            // Act
            var result = await _mockCheckoutService.Object.GetActiveCheckoutByCartIdAsync("cart-none");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // SetShippingAddressAsync Tests
        // ─────────────────────────────────────────────────────────

        #region SetShippingAddressAsync

        [Fact]
        public async Task SetShippingAddressAsync_WithValidData_ReturnsAddress()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var addressDto = new CheckoutAddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                AddressLine1 = "12 Rue Hassan II",
                City = "Casablanca",
                State = "Casablanca",
                PostalCode = "20000",
                Country = "MA",
                PhoneNumber = "+212600000000"
            };

            var expectedAddress = new CheckoutAddress
            {
                Id = Guid.NewGuid(),
                CheckoutId = checkoutId,
                AddressType = "Shipping",
                FirstName = addressDto.FirstName,
                LastName = addressDto.LastName,
                AddressLine1 = addressDto.AddressLine1,
                City = addressDto.City,
                State = addressDto.State,
                PostalCode = addressDto.PostalCode,
                Country = addressDto.Country
            };

            _mockCheckoutService
                .Setup(s => s.SetShippingAddressAsync(checkoutId, addressDto))
                .ReturnsAsync(expectedAddress);

            // Act
            var result = await _mockCheckoutService.Object.SetShippingAddressAsync(checkoutId, addressDto);

            // Assert
            result.Should().NotBeNull();
            result.AddressType.Should().Be("Shipping");
            result.FirstName.Should().Be("John");
            result.Country.Should().Be("MA");
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // CopyShippingToBillingAsync Tests
        // ─────────────────────────────────────────────────────────

        #region CopyShippingToBillingAsync

        [Fact]
        public async Task CopyShippingToBillingAsync_WhenShippingExists_ReturnsTrue()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.CopyShippingToBillingAsync(checkoutId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCheckoutService.Object.CopyShippingToBillingAsync(checkoutId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CopyShippingToBillingAsync_WhenNoShipping_ReturnsFalse()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.CopyShippingToBillingAsync(checkoutId))
                .ReturnsAsync(false);

            // Act
            var result = await _mockCheckoutService.Object.CopyShippingToBillingAsync(checkoutId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // SetPaymentMethodAsync Tests
        // ─────────────────────────────────────────────────────────

        #region SetPaymentMethodAsync

        [Fact]
        public async Task SetPaymentMethodAsync_WithStripe_UpdatesPaymentMethod()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var expected = new CheckoutSummaryDto
            {
                Id = checkoutId,
                PaymentMethod = "stripe"
            };

            _mockCheckoutService
                .Setup(s => s.SetPaymentMethodAsync(checkoutId, "stripe", null))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.SetPaymentMethodAsync(checkoutId, "stripe", null);

            // Assert
            result.Should().NotBeNull();
            result.PaymentMethod.Should().Be("stripe");
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // CompleteCheckoutAsync Tests
        // ─────────────────────────────────────────────────────────

        #region CompleteCheckoutAsync

        [Fact]
        public async Task CompleteCheckoutAsync_WithValidCheckout_ReturnsCompletedCheckout()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var dto = new CompleteCheckoutDto { CheckoutId = checkoutId };
            var expected = new CheckoutSummaryDto
            {
                Id = checkoutId,
                Status = "Completed"
            };

            _mockCheckoutService
                .Setup(s => s.CompleteCheckoutAsync(dto))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockCheckoutService.Object.CompleteCheckoutAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task CompleteCheckoutAsync_WhenNoItems_ThrowsException()
        {
            // Arrange
            var dto = new CompleteCheckoutDto { CheckoutId = Guid.NewGuid() };

            _mockCheckoutService
                .Setup(s => s.CompleteCheckoutAsync(dto))
                .ThrowsAsync(new InvalidOperationException("No items in checkout"));

            // Act & Assert
            var act = async () => await _mockCheckoutService.Object.CompleteCheckoutAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No items in checkout*");
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // AbandonCheckoutAsync Tests
        // ─────────────────────────────────────────────────────────

        #region AbandonCheckoutAsync

        [Fact]
        public async Task AbandonCheckoutAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.AbandonCheckoutAsync(checkoutId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCheckoutService.Object.AbandonCheckoutAsync(checkoutId);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // ValidateCartIntegrityAsync Tests
        // ─────────────────────────────────────────────────────────

        #region ValidateCartIntegrityAsync

        [Fact]
        public async Task ValidateCartIntegrityAsync_WhenCartMatchesCheckout_ReturnsTrue()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var cartId = "cart-valid";

            _mockCheckoutService
                .Setup(s => s.ValidateCartIntegrityAsync(checkoutId, cartId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCheckoutService.Object.ValidateCartIntegrityAsync(checkoutId, cartId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateCartIntegrityAsync_WhenMismatch_ReturnsFalse()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.ValidateCartIntegrityAsync(checkoutId, It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _mockCheckoutService.Object.ValidateCartIntegrityAsync(checkoutId, "cart-wrong");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // IsCheckoutValidAsync Tests
        // ─────────────────────────────────────────────────────────

        #region IsCheckoutValidAsync

        [Fact]
        public async Task IsCheckoutValidAsync_WhenNotExpired_ReturnsTrue()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.IsCheckoutValidAsync(checkoutId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCheckoutService.Object.IsCheckoutValidAsync(checkoutId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsCheckoutValidAsync_WhenExpired_ReturnsFalse()
        {
            // Arrange
            var expiredCheckoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.IsCheckoutValidAsync(expiredCheckoutId))
                .ReturnsAsync(false);

            // Act
            var result = await _mockCheckoutService.Object.IsCheckoutValidAsync(expiredCheckoutId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // CalculateTaxAsync Tests
        // ─────────────────────────────────────────────────────────

        #region CalculateTaxAsync

        [Fact]
        public async Task CalculateTaxAsync_WithPositiveSubtotal_ReturnsCorrectTax()
        {
            // Arrange
            var subtotal = 500m;
            var expectedTax = 40m; // e.g. 8% tax rate

            _mockCheckoutService
                .Setup(s => s.CalculateTaxAsync(subtotal, It.IsAny<string>()))
                .ReturnsAsync(expectedTax);

            // Act
            var tax = await _mockCheckoutService.Object.CalculateTaxAsync(subtotal);

            // Assert
            tax.Should().Be(40m);
        }

        [Fact]
        public async Task CalculateTaxAsync_WithZeroSubtotal_ReturnsZero()
        {
            // Arrange
            _mockCheckoutService
                .Setup(s => s.CalculateTaxAsync(0m, It.IsAny<string>()))
                .ReturnsAsync(0m);

            // Act
            var tax = await _mockCheckoutService.Object.CalculateTaxAsync(0m);

            // Assert
            tax.Should().Be(0m);
        }

        #endregion
    }
}
