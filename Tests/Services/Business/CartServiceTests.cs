using System;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;
using ALOud.Services;
using ALOud.Services.Infrastructure.Cache;
using ViewModels;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for CartService using Strategy A (Mock-based).
    /// Tests the cart business logic with mocked cache and HTTP context dependencies.
    /// 
    /// Test naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class CartServiceTests
    {
        private readonly CartService _cartService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;

        public CartServiceTests()
        {
            // Arrange - Mock all external dependencies
            _mockCacheService = new Mock<ICacheService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            
            // Setup HttpContextAccessor to return our mock context
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
            
            // Create service under test
            _cartService = new CartService(_mockCacheService.Object, _mockHttpContextAccessor.Object);
        }

        #region GetCartAsync Tests

        [Fact]
        public async Task GetCartAsync_WhenCacheHasCart_ReturnsCartFromCache()
        {
            // Arrange
            var expectedCartItems = new List<CartItemVM>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Perfume 1", Quantity = 2, Price = 50.0m },
                new() { ProductId = Guid.NewGuid(), ProductName = "Perfume 2", Quantity = 1, Price = 75.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(expectedCartItems);

            // Act
            var result = await _cartService.GetCartAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedCartItems);
            
            // Verify cache was called
            _mockCacheService.Verify(
                cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetCartAsync_WhenCacheReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync((List<CartItemVM>?)null);

            // Act
            var result = await _cartService.GetCartAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region SaveCartAsync Tests

        [Fact]
        public async Task SaveCartAsync_WhenCalledWithCart_SavesCartToCacheWithCorrectExpiry()
        {
            // Arrange
            var cartItems = new List<CartItemVM>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Test Perfume", Quantity = 1, Price = 100.0m }
            };

            // Act
            await _cartService.SaveCartAsync(cartItems);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    cartItems, 
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        [Fact]
        public async Task SaveCartAsync_WhenCalledWithEmptyCart_SavesEmptyCartToCache()
        {
            // Arrange
            var emptyCart = new List<CartItemVM>();

            // Act
            await _cartService.SaveCartAsync(emptyCart);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    emptyCart, 
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        #endregion

        #region AddToCartAsync Tests

        [Fact]
        public async Task AddToCartAsync_WhenItemNotInCart_AddsItemToCart()
        {
            // Arrange
            var existingCart = new List<CartItemVM>();
            var newItem = new CartItemVM 
            { 
                ProductId = Guid.NewGuid(), 
                ProductName = "New Perfume", 
                Quantity = 1, 
                Price = 80.0m 
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.AddToCartAsync(newItem);

            // Assert
            // Verify the cart was retrieved
            _mockCacheService.Verify(
                cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()), 
                Times.Once);

            // Verify the updated cart was saved with the new item
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == newItem.ProductId &&
                        cart[0].Quantity == 1), 
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WhenItemAlreadyInCart_IncrementsQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = productId, ProductName = "Existing Perfume", Quantity = 2, Price = 60.0m }
            };
            
            var itemToAdd = new CartItemVM 
            { 
                ProductId = productId, 
                ProductName = "Existing Perfume", 
                Quantity = 1, 
                Price = 60.0m 
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.AddToCartAsync(itemToAdd);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == productId &&
                        cart[0].Quantity == 3), // Should be incremented from 2 to 3
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        #endregion

        #region RemoveAsync Tests

        [Fact]
        public async Task RemoveAsync_WhenItemInCart_RemovesItemFromCart()
        {
            // Arrange
            var productIdToRemove = Guid.NewGuid();
            var productIdToKeep = Guid.NewGuid();
            
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = productIdToRemove, ProductName = "Remove Me", Quantity = 1, Price = 50.0m },
                new() { ProductId = productIdToKeep, ProductName = "Keep Me", Quantity = 2, Price = 75.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.RemoveAsync(productIdToRemove);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == productIdToKeep), 
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_WhenItemNotInCart_CartUnchanged()
        {
            // Arrange
            var existingProductId = Guid.NewGuid();
            var nonExistentProductId = Guid.NewGuid();
            
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = existingProductId, ProductName = "Existing Item", Quantity = 1, Price = 50.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.RemoveAsync(nonExistentProductId);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == existingProductId), 
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        #endregion

        #region IncreaseAsync Tests

        [Fact]
        public async Task IncreaseAsync_WhenItemInCart_IncrementsQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = productId, ProductName = "Test Item", Quantity = 3, Price = 45.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.IncreaseAsync(productId);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == productId &&
                        cart[0].Quantity == 4), // Should be incremented from 3 to 4
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        #endregion

        #region DecreaseAsync Tests

        [Fact]
        public async Task DecreaseAsync_WhenQuantityGreaterThanOne_DecrementsQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = productId, ProductName = "Test Item", Quantity = 3, Price = 45.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.DecreaseAsync(productId);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => 
                        cart.Count == 1 && 
                        cart[0].ProductId == productId &&
                        cart[0].Quantity == 2), // Should be decremented from 3 to 2
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        [Fact]
        public async Task DecreaseAsync_WhenQuantityIsOne_RemovesItemFromCart()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingCart = new List<CartItemVM>
            {
                new() { ProductId = productId, ProductName = "Test Item", Quantity = 1, Price = 45.0m }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(existingCart);

            // Act
            await _cartService.DecreaseAsync(productId);

            // Assert
            _mockCacheService.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(), 
                    It.Is<List<CartItemVM>>(cart => cart.Count == 0), // Item should be removed
                    TimeSpan.FromDays(7)), 
                Times.Once);
        }

        #endregion

        #region GetCartItemCountAsync Tests

        [Fact]
        public async Task GetCartItemCountAsync_WhenCartHasItems_ReturnsTotalQuantity()
        {
            // Arrange
            var cartItems = new List<CartItemVM>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 3 },
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            };

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(cartItems);

            // Act
            var result = await _cartService.GetCartItemCountAsync();

            // Assert
            result.Should().Be(6); // 2 + 3 + 1 = 6
        }

        [Fact]
        public async Task GetCartItemCountAsync_WhenCartIsEmpty_ReturnsZero()
        {
            // Arrange
            var emptyCart = new List<CartItemVM>();

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync(emptyCart);

            // Act
            var result = await _cartService.GetCartItemCountAsync();

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetCartItemCountAsync_WhenCacheReturnsNull_ReturnsZero()
        {
            // Arrange
            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .ReturnsAsync((List<CartItemVM>?)null);

            // Act
            var result = await _cartService.GetCartItemCountAsync();

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task AddThenIncreaseThenDecreaseThenRemove_FullWorkflow_BehavesCorrectly()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var emptyCart = new List<CartItemVM>();
            var item = new CartItemVM 
            { 
                ProductId = productId, 
                ProductName = "Workflow Item", 
                Quantity = 1, 
                Price = 50.0m 
            };

            // Setup cache to return different states for each operation
            var cartStates = new Queue<List<CartItemVM>>();
            cartStates.Enqueue(emptyCart); // For AddToCartAsync
            cartStates.Enqueue(new List<CartItemVM> { new() { ProductId = productId, Quantity = 1, Price = 50.0m } }); // For IncreaseAsync
            cartStates.Enqueue(new List<CartItemVM> { new() { ProductId = productId, Quantity = 2, Price = 50.0m } }); // For DecreaseAsync
            cartStates.Enqueue(new List<CartItemVM> { new() { ProductId = productId, Quantity = 1, Price = 50.0m } }); // For RemoveAsync

            _mockCacheService
                .Setup(cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()))
                .Returns(() => Task.FromResult<List<CartItemVM>?>(cartStates.Dequeue()));

            // Act & Assert - Add item
            await _cartService.AddToCartAsync(item);
            
            // Act & Assert - Increase quantity
            await _cartService.IncreaseAsync(productId);
            
            // Act & Assert - Decrease quantity
            await _cartService.DecreaseAsync(productId);
            
            // Act & Assert - Remove item
            await _cartService.RemoveAsync(productId);

            // Verify all cache operations were called
            _mockCacheService.Verify(
                cache => cache.GetAsync<List<CartItemVM>>(It.IsAny<string>()), 
                Times.Exactly(4));
                
            _mockCacheService.Verify(
                cache => cache.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemVM>>(), It.IsAny<TimeSpan>()), 
                Times.Exactly(4));
        }

        #endregion
    }
}