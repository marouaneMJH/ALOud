using ALOud.DTOs;
using ALOud.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for IOrderService.
    /// Covers order creation, status transitions, fulfillment, and validation logic.
    ///
    /// Naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class OrderServiceTests
    {
        private readonly Mock<IOrderService> _mockOrderService;

        public OrderServiceTests()
        {
            _mockOrderService = new Mock<IOrderService>();
        }

        // ────────────────────────────────────────────────────────────
        // GenerateOrderNumber Tests
        // ────────────────────────────────────────────────────────────

        #region GenerateOrderNumber

        [Fact]
        public void GenerateOrderNumber_ReturnsCorrectFormat()
        {
            // Arrange
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            _mockOrderService
                .Setup(s => s.GenerateOrderNumber())
                .Returns($"ALO-{today}-ABCD1234");

            // Act
            var orderNumber = _mockOrderService.Object.GenerateOrderNumber();

            // Assert
            orderNumber.Should().StartWith("ALO-");
            orderNumber.Length.Should().BeLessOrEqualTo(30); // must fit inside nvarchar(30)
            orderNumber.Should().MatchRegex(@"^ALO-\d{8}-[A-Z0-9]{8}$");
        }

        [Fact]
        public void GenerateOrderNumber_EachCallReturnsUnique()
        {
            // Arrange
            var serviceImpl = new Mock<IOrderService>();
            serviceImpl
                .Setup(s => s.GenerateOrderNumber())
                .Returns(() => $"ALO-20260424-{Guid.NewGuid().ToString("N")[..8].ToUpper()}");

            // Act
            var results = Enumerable.Range(0, 100)
                .Select(_ => serviceImpl.Object.GenerateOrderNumber())
                .ToList();

            // Assert — all 100 should be unique
            results.Should().OnlyHaveUniqueItems();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // CreateOrderFromCheckoutAsync Tests
        // ────────────────────────────────────────────────────────────

        #region CreateOrderFromCheckoutAsync

        [Fact]
        public async Task CreateOrderFromCheckoutAsync_WithValidCheckout_ReturnsOrder()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();
            var expected = new OrderDto
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ALO-20260424-ABCD1234",
                Status = "Pending",
                CustomerEmail = "user@example.com",
                TotalAmount = 350m
            };

            _mockOrderService
                .Setup(s => s.CreateOrderFromCheckoutAsync(checkoutId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockOrderService.Object.CreateOrderFromCheckoutAsync(checkoutId);

            // Assert
            result.Should().NotBeNull();
            result.OrderNumber.Should().StartWith("ALO-");
            result.Status.Should().Be("Pending");
            result.TotalAmount.Should().Be(350m);

            _mockOrderService.Verify(s => s.CreateOrderFromCheckoutAsync(checkoutId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderFromCheckoutAsync_WithNoReservations_ThrowsException()
        {
            // Arrange
            var checkoutId = Guid.NewGuid();

            _mockOrderService
                .Setup(s => s.CreateOrderFromCheckoutAsync(checkoutId))
                .ThrowsAsync(new InvalidOperationException("No confirmed stock reservations found for this checkout."));

            // Act & Assert
            var act = async () => await _mockOrderService.Object.CreateOrderFromCheckoutAsync(checkoutId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*stock reservations*");
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // GetOrderAsync Tests
        // ────────────────────────────────────────────────────────────

        #region GetOrderAsync

        [Fact]
        public async Task GetOrderAsync_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var expected = new OrderDto
            {
                Id = orderId,
                OrderNumber = "ALO-20260424-XXXXXX",
                Status = "Paid",
                CustomerEmail = "test@example.com"
            };

            _mockOrderService
                .Setup(s => s.GetOrderAsync(orderId))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockOrderService.Object.GetOrderAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(orderId);
            result.Status.Should().Be("Paid");
        }

        [Fact]
        public async Task GetOrderAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockOrderService
                .Setup(s => s.GetOrderAsync(It.IsAny<Guid>()))
                .ReturnsAsync((OrderDto?)null);

            // Act
            var result = await _mockOrderService.Object.GetOrderAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // GetOrderByNumberAsync Tests
        // ────────────────────────────────────────────────────────────

        #region GetOrderByNumberAsync

        [Fact]
        public async Task GetOrderByNumberAsync_WithValidNumber_ReturnsOrder()
        {
            // Arrange
            var orderNumber = "ALO-20260424-ABCD1234";
            var expected = new OrderDto { OrderNumber = orderNumber, Status = "Processing" };

            _mockOrderService
                .Setup(s => s.GetOrderByNumberAsync(orderNumber))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockOrderService.Object.GetOrderByNumberAsync(orderNumber);

            // Assert
            result.Should().NotBeNull();
            result!.OrderNumber.Should().Be(orderNumber);
        }

        [Fact]
        public async Task GetOrderByNumberAsync_WithUnknownNumber_ReturnsNull()
        {
            // Arrange
            _mockOrderService
                .Setup(s => s.GetOrderByNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((OrderDto?)null);

            // Act
            var result = await _mockOrderService.Object.GetOrderByNumberAsync("ALO-INVALID-NUM");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // GetUserOrdersAsync Tests
        // ────────────────────────────────────────────────────────────

        #region GetUserOrdersAsync

        [Fact]
        public async Task GetUserOrdersAsync_WhenUserHasOrders_ReturnsList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expected = new List<OrderSummaryDto>
            {
                new() { Id = Guid.NewGuid(), OrderNumber = "ALO-20260424-A1", Status = "Delivered" },
                new() { Id = Guid.NewGuid(), OrderNumber = "ALO-20260424-A2", Status = "Pending" }
            };

            _mockOrderService
                .Setup(s => s.GetUserOrdersAsync(userId, 1, 20))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockOrderService.Object.GetUserOrdersAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(o => o.Status == "Delivered");
        }

        [Fact]
        public async Task GetUserOrdersAsync_WhenNoOrders_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockOrderService
                .Setup(s => s.GetUserOrdersAsync(userId, 1, 20))
                .ReturnsAsync(new List<OrderSummaryDto>());

            // Act
            var result = await _mockOrderService.Object.GetUserOrdersAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // UpdateOrderStatusAsync Tests
        // ────────────────────────────────────────────────────────────

        #region UpdateOrderStatusAsync

        [Fact]
        public async Task UpdateOrderStatusAsync_WithValidTransition_ReturnsTrue()
        {
            // Arrange
            var dto = new UpdateOrderStatusDto
            {
                OrderId = Guid.NewGuid(),
                NewStatus = "Processing",
                ChangeReason = "Payment confirmed"
            };

            _mockOrderService
                .Setup(s => s.UpdateOrderStatusAsync(dto, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockOrderService.Object.UpdateOrderStatusAsync(dto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithInvalidTransition_ReturnsFalse()
        {
            // Arrange
            var dto = new UpdateOrderStatusDto
            {
                OrderId = Guid.NewGuid(),
                NewStatus = "Pending", // can't go backwards
                ChangeReason = "Test"
            };

            _mockOrderService
                .Setup(s => s.UpdateOrderStatusAsync(dto, null))
                .ReturnsAsync(false);

            // Act
            var result = await _mockOrderService.Object.UpdateOrderStatusAsync(dto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // CancelOrderAsync Tests
        // ────────────────────────────────────────────────────────────

        #region CancelOrderAsync

        [Fact]
        public async Task CancelOrderAsync_WhenCancellable_ReturnsTrue()
        {
            // Arrange
            var dto = new CancelOrderDto { OrderId = Guid.NewGuid(), Reason = "Customer request" };

            _mockOrderService
                .Setup(s => s.CancelOrderAsync(dto, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockOrderService.Object.CancelOrderAsync(dto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CancelOrderAsync_WhenAlreadyShipped_ReturnsFalse()
        {
            // Arrange
            var dto = new CancelOrderDto { OrderId = Guid.NewGuid(), Reason = "Changed mind" };

            _mockOrderService
                .Setup(s => s.CancelOrderAsync(dto, null))
                .ReturnsAsync(false);

            // Act
            var result = await _mockOrderService.Object.CancelOrderAsync(dto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // MarkOrderAsPaidAsync Tests
        // ────────────────────────────────────────────────────────────

        #region MarkOrderAsPaidAsync

        [Fact]
        public async Task MarkOrderAsPaidAsync_WithValidPayment_ReturnsTrue()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockOrderService
                .Setup(s => s.MarkOrderAsPaidAsync(orderId, "pi_test123", 500m))
                .ReturnsAsync(true);

            // Act
            var result = await _mockOrderService.Object.MarkOrderAsPaidAsync(orderId, "pi_test123", 500m);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // IsValidOrderStatusTransitionAsync Tests
        // ────────────────────────────────────────────────────────────

        #region IsValidOrderStatusTransitionAsync

        [Theory]
        [InlineData("Pending", "Paid", true)]
        [InlineData("Paid", "Processing", true)]
        [InlineData("Processing", "Shipped", true)]
        [InlineData("Shipped", "Delivered", true)]
        [InlineData("Pending", "Delivered", false)] // illegal skip
        [InlineData("Delivered", "Pending", false)]  // illegal reversal
        public async Task IsValidOrderStatusTransitionAsync_AllScenarios(
            string current, string next, bool expected)
        {
            // Arrange
            _mockOrderService
                .Setup(s => s.IsValidOrderStatusTransitionAsync(current, next))
                .ReturnsAsync(expected);

            // Act
            var result = await _mockOrderService.Object.IsValidOrderStatusTransitionAsync(current, next);

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // CanCancelOrderAsync / CanRefundOrderAsync Tests
        // ────────────────────────────────────────────────────────────

        #region CanCancelOrderAsync / CanRefundOrderAsync

        [Fact]
        public async Task CanCancelOrderAsync_WhenPending_ReturnsTrue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.CanCancelOrderAsync(orderId)).ReturnsAsync(true);

            // Act & Assert
            var result = await _mockOrderService.Object.CanCancelOrderAsync(orderId);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanRefundOrderAsync_WhenDelivered_ReturnsTrue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.CanRefundOrderAsync(orderId)).ReturnsAsync(true);

            // Act & Assert
            var result = await _mockOrderService.Object.CanRefundOrderAsync(orderId);
            result.Should().BeTrue();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // ValidateOrderOwnershipAsync Tests
        // ────────────────────────────────────────────────────────────

        #region ValidateOrderOwnershipAsync

        [Fact]
        public async Task ValidateOrderOwnershipAsync_WhenUserOwnsOrder_ReturnsTrue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockOrderService
                .Setup(s => s.ValidateOrderOwnershipAsync(orderId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockOrderService.Object.ValidateOrderOwnershipAsync(orderId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateOrderOwnershipAsync_WhenUserDoesNotOwnOrder_ReturnsFalse()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();

            _mockOrderService
                .Setup(s => s.ValidateOrderOwnershipAsync(orderId, wrongUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _mockOrderService.Object.ValidateOrderOwnershipAsync(orderId, wrongUserId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        // ────────────────────────────────────────────────────────────
        // GetPendingOrdersCountAsync Tests
        // ────────────────────────────────────────────────────────────

        #region GetPendingOrdersCountAsync

        [Fact]
        public async Task GetPendingOrdersCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            _mockOrderService.Setup(s => s.GetPendingOrdersCountAsync()).ReturnsAsync(7);

            // Act
            var count = await _mockOrderService.Object.GetPendingOrdersCountAsync();

            // Assert
            count.Should().Be(7);
        }

        #endregion
    }
}
