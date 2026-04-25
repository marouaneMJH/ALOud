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
    /// <summary>
    /// Unit tests for the API CheckoutController (v1).
    /// Each test exercises one controller action in isolation using mocked services.
    ///
    /// Naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class CheckoutControllerTests
    {
        private readonly Mock<ICheckoutService> _mockCheckoutService;
        private readonly Mock<ICartService> _mockCartService;
        private readonly Mock<ILogger<CheckoutController>> _mockLogger;
        private readonly CheckoutController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public CheckoutControllerTests()
        {
            _mockCheckoutService = new Mock<ICheckoutService>();
            _mockCartService = new Mock<ICartService>();
            _mockLogger = new Mock<ILogger<CheckoutController>>();

            _controller = new CheckoutController(
                _mockCheckoutService.Object,
                _mockCartService.Object,
                _mockLogger.Object);

            // Setup authenticated user context with cookie-based CartId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Email, "user@example.com")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // HttpContext — must set cookies via RequestCookieCollection for GetCartId() to work
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            httpContext.Request.Headers["Cookie"] = "CartId=cart-test-abc";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Helper: returns a valid CheckoutSummaryDto for a given id to satisfy ValidateCheckoutAccess guard.
        /// Authenticated user context means ValidateCheckoutAccess always returns true — but the controller
        /// first calls GetCheckoutAsync(id) and checks it's non-null, so we must mock it.
        /// </summary>
        private CheckoutSummaryDto BuildCheckoutSummary(Guid id) => new CheckoutSummaryDto
        {
            Id = id,
            Email = "user@example.com",
            Status = "InProgress",
            IsGuestCheckout = false
        };

        // ─────────────────────────────────────────────────────────
        // StartCheckout Tests
        // ─────────────────────────────────────────────────────────

        #region StartCheckout

        [Fact]
        public async Task StartCheckout_WithValidDto_ReturnsCreated()
        {
            // Arrange
            var dto = new StartCheckoutDto { Email = "user@example.com", IsGuestCheckout = false };
            var checkoutSummary = new CheckoutSummaryDto
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Status = "InProgress"
            };

            _mockCheckoutService
                .Setup(s => s.StartCheckoutAsync(It.IsAny<StartCheckoutDto>(), It.IsAny<string>()))
                .ReturnsAsync(checkoutSummary);

            // Act
            var result = await _controller.StartCheckout(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(created.Value);
            _mockCheckoutService.Verify(
                s => s.StartCheckoutAsync(It.IsAny<StartCheckoutDto>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task StartCheckout_WithInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            var dto = new StartCheckoutDto { Email = "user@example.com" };

            _mockCheckoutService
                .Setup(s => s.StartCheckoutAsync(It.IsAny<StartCheckoutDto>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Insufficient stock"));

            // Act
            var result = await _controller.StartCheckout(dto);

            // Assert
            var badRequest = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // GetCheckout Tests
        // ─────────────────────────────────────────────────────────

        #region GetCheckout

        [Fact]
        public async Task GetCheckout_WithValidId_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var summary = BuildCheckoutSummary(checkoutId);

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(summary);

            // Act
            var result = await _controller.GetCheckout(checkoutId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetCheckout_WhenCheckoutNotFound_ReturnsNotFound()
        {
            // Arrange
            var missingId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(missingId))
                .ReturnsAsync((CheckoutSummaryDto?)null);

            // Act
            var result = await _controller.GetCheckout(missingId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // GetCurrentCheckout Tests
        // ─────────────────────────────────────────────────────────

        #region GetCurrentCheckout

        [Fact]
        public async Task GetCurrentCheckout_WhenActiveExists_ReturnsOk()
        {
            // Arrange
            var summary = new CheckoutSummaryDto { Id = Guid.NewGuid(), Status = "InProgress" };

            _mockCheckoutService
                .Setup(s => s.GetActiveCheckoutByCartIdAsync(It.IsAny<string>()))
                .ReturnsAsync(summary);

            // Act
            var result = await _controller.GetCurrentCheckout();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetCurrentCheckout_WhenNoneExists_ReturnsNotFound()
        {
            // Arrange
            _mockCheckoutService
                .Setup(s => s.GetActiveCheckoutByCartIdAsync(It.IsAny<string>()))
                .ReturnsAsync((CheckoutSummaryDto?)null);

            // Act
            var result = await _controller.GetCurrentCheckout();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // SetShippingAddress Tests
        // ─────────────────────────────────────────────────────────

        #region SetShippingAddress

        [Fact]
        public async Task SetShippingAddress_WithValidData_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            // Guard: GetCheckoutAsync must return non-null for ValidateCheckoutAccess
            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));

            var dto = new CheckoutAddressDto
            {
                FirstName = "Yassine",
                LastName = "Bensaid",
                AddressLine1 = "34 Av Mohammed V",
                City = "Rabat",
                State = "Rabat",
                PostalCode = "10000",
                Country = "MA",
                PhoneNumber = "+212612345678"
            };
            var address = new ALOud.Models.CheckoutAddress
            {
                Id = Guid.NewGuid(),
                CheckoutId = checkoutId,
                AddressType = "Shipping",
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                AddressLine1 = dto.AddressLine1,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country
            };

            _mockCheckoutService
                .Setup(s => s.SetShippingAddressAsync(checkoutId, dto))
                .ReturnsAsync(address);

            // Act
            var result = await _controller.SetShippingAddress(checkoutId, dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        #endregion

       

        // ─────────────────────────────────────────────────────────
        // SetShippingMethod Tests
        // ─────────────────────────────────────────────────────────

        #region SetShippingMethod

        [Fact]
        public async Task SetShippingMethod_WithValidMethod_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var expectedSummary = new CheckoutSummaryDto { Id = checkoutId, ShippingMethod = "standard" };

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.SetShippingMethodAsync(checkoutId, "standard"))
                .ReturnsAsync(expectedSummary);

            // Act
            var result = await _controller.SetShippingMethod(checkoutId, "standard");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // SetPaymentMethod Tests
        // ─────────────────────────────────────────────────────────

        #region SetPaymentMethod

        [Fact]
        public async Task SetPaymentMethod_WithStripe_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var request = new CheckoutController.SetPaymentMethodRequest
            {
                PaymentMethod = "stripe",
                PaymentIntentId = null
            };

            var expectedSummary = new CheckoutSummaryDto { Id = checkoutId, PaymentMethod = "stripe" };

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.SetPaymentMethodAsync(checkoutId, "stripe", null))
                .ReturnsAsync(expectedSummary);

            // Act
            var result = await _controller.SetPaymentMethod(checkoutId, request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // CompleteCheckout Tests
        // ─────────────────────────────────────────────────────────

        #region CompleteCheckout

        [Fact]
        public async Task CompleteCheckout_WithValidCheckout_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var dto = new CompleteCheckoutDto { CheckoutId = checkoutId };
            var expected = new CheckoutSummaryDto { Id = checkoutId, Status = "Completed" };

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.CompleteCheckoutAsync(dto))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.CompleteCheckout(checkoutId, dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task CompleteCheckout_WhenCheckoutIdMismatch_ReturnsBadRequest()
        {
            // Arrange — URL checkoutId does not match dto.CheckoutId
            var routeId = Guid.NewGuid();
            var dto = new CompleteCheckoutDto { CheckoutId = Guid.NewGuid() };

            // Act
            var result = await _controller.CompleteCheckout(routeId, dto);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var resultCode = (result as ObjectResult)?.StatusCode;
            Assert.Equal(400, resultCode);
        }

        [Fact]
        public async Task CompleteCheckout_WhenServiceThrows_Returns500()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var dto = new CompleteCheckoutDto { CheckoutId = checkoutId };

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.CompleteCheckoutAsync(dto))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.CompleteCheckout(checkoutId, dto);

            // Assert
            var serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        #endregion

        // ─────────────────────────────────────────────────────────
        // AbandonCheckout Tests
        // ─────────────────────────────────────────────────────────

        #region AbandonCheckout

        [Fact]
        public async Task AbandonCheckout_WithValidId_ReturnsOk()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.AbandonCheckoutAsync(checkoutId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AbandonCheckout(checkoutId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task AbandonCheckout_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockCheckoutService
                .Setup(s => s.GetCheckoutAsync(checkoutId))
                .ReturnsAsync(BuildCheckoutSummary(checkoutId));
            _mockCheckoutService
                .Setup(s => s.AbandonCheckoutAsync(checkoutId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AbandonCheckout(checkoutId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion
    }
}
