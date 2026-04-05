using ALOud.Controllers.Api;
using ALOud.Services;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for stock and inventory management (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StockController : BaseApiController
    {
        private readonly IStockReservationService _stockService;
        private readonly ILogger<StockController> _logger;

        /// <summary>
        /// Initializes a new instance of the StockController class
        /// </summary>
        /// <param name="stockService">The stock reservation service</param>
        /// <param name="logger">The logger</param>
        public StockController(
            IStockReservationService stockService,
            ILogger<StockController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// Validates stock availability for cart items
        /// </summary>
        /// <param name="cartItems">List of cart items to validate</param>
        /// <returns>Stock validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(StockValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateStock([FromBody] List<CartItemVM> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
            {
                return ErrorResponse("Cart items are required for validation");
            }

            try
            {
                var isAvailable = await _stockService.ValidateStockAvailabilityAsync(cartItems);
                var availableStock = await _stockService.GetAvailableStockAsync(
                    cartItems.Select(item => item.ProductId).ToList()
                );

                var result = new StockValidationResult
                {
                    IsAvailable = isAvailable,
                    AvailableStock = availableStock,
                    ValidationDetails = cartItems.Select(item => new StockValidationDetail
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = availableStock.ContainsKey(item.ProductId) 
                            ? availableStock[item.ProductId] 
                            : 0,
                        IsAvailable = availableStock.ContainsKey(item.ProductId) && 
                                     availableStock[item.ProductId] >= item.Quantity
                    }).ToList()
                };

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stock availability");
                return ErrorResponse("Failed to validate stock availability", 500);
            }
        }

        /// <summary>
        /// Gets available stock levels for specific products
        /// </summary>
        /// <param name="productIds">List of product IDs to check</param>
        /// <returns>Dictionary of product ID to available stock quantity</returns>
        [HttpPost("availability")]
        [ProducesResponseType(typeof(Dictionary<Guid, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableStock([FromBody] List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return ErrorResponse("Product IDs are required");
            }

            try
            {
                var stockLevels = await _stockService.GetAvailableStockAsync(productIds);
                return SuccessResponse(stockLevels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available stock levels");
                return ErrorResponse("Failed to get stock levels", 500);
            }
        }

        /// <summary>
        /// Gets available stock for a single product
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>Available stock quantity</returns>
        [HttpGet("{productId:guid}/availability")]
        [ProducesResponseType(typeof(ProductStockInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductStock(Guid productId)
        {
            try
            {
                var stockLevels = await _stockService.GetAvailableStockAsync(new List<Guid> { productId });
                
                if (!stockLevels.ContainsKey(productId))
                {
                    return NotFoundResponse("Product not found");
                }

                var activeReservations = await _stockService.GetActiveReservationsAsync(productId);
                
                var stockInfo = new ProductStockInfo
                {
                    ProductId = productId,
                    AvailableQuantity = stockLevels[productId],
                    TotalReserved = activeReservations.Sum(r => r.Quantity),
                    ActiveReservations = activeReservations.Count
                };

                return SuccessResponse(stockInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock info for product: {ProductId}", productId);
                return ErrorResponse("Failed to get product stock info", 500);
            }
        }

        /// <summary>
        /// Checks if enough stock is available for a specific quantity of a product
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="quantity">The requested quantity</param>
        /// <returns>Whether the stock is available</returns>
        [HttpGet("{productId:guid}/check/{quantity:int}")]
        [ProducesResponseType(typeof(StockCheckResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckStockAvailability(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                return ErrorResponse("Quantity must be greater than 0");
            }

            try
            {
                var isAvailable = await _stockService.IsStockReservedAsync(productId, quantity);
                var stockLevels = await _stockService.GetAvailableStockAsync(new List<Guid> { productId });
                
                var result = new StockCheckResult
                {
                    ProductId = productId,
                    RequestedQuantity = quantity,
                    IsAvailable = isAvailable,
                    AvailableQuantity = stockLevels.ContainsKey(productId) ? stockLevels[productId] : 0
                };

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability for product {ProductId}, quantity {Quantity}", 
                    productId, quantity);
                return ErrorResponse("Failed to check stock availability", 500);
            }
        }

        /// <summary>
        /// Gets active reservations for a product (admin/debug use)
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <returns>List of active reservations</returns>
        [HttpGet("{productId:guid}/reservations")]
        [ProducesResponseType(typeof(List<StockReservationInfo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveReservations(Guid productId)
        {
            try
            {
                var reservations = await _stockService.GetActiveReservationsAsync(productId);
                
                var reservationInfo = reservations.Select(r => new StockReservationInfo
                {
                    Id = r.Id,
                    CheckoutId = r.CheckoutId,
                    Quantity = r.Quantity,
                    UnitPrice = r.UnitPrice,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    ReservedAt = r.ReservedAt,
                    ExpiresAt = r.ExpiresAt
                }).ToList();

                return SuccessResponse(reservationInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active reservations for product: {ProductId}", productId);
                return ErrorResponse("Failed to get reservations", 500);
            }
        }

        /// <summary>
        /// Cleans up expired stock reservations (admin/maintenance use)
        /// </summary>
        /// <returns>Number of reservations cleaned up</returns>
        [HttpPost("cleanup-expired")]
        [ProducesResponseType(typeof(CleanupResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> CleanupExpiredReservations()
        {
            try
            {
                var cleanedCount = await _stockService.CleanupExpiredReservationsAsync();
                
                var result = new CleanupResult
                {
                    ReservationsCleaned = cleanedCount,
                    CleanedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Cleaned up {Count} expired reservations", cleanedCount);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired reservations");
                return ErrorResponse("Failed to cleanup expired reservations", 500);
            }
        }
    }

    #region Response Models

    /// <summary>
    /// Stock validation result for multiple products
    /// </summary>
    public class StockValidationResult
    {
        public bool IsAvailable { get; set; }
        public Dictionary<Guid, int> AvailableStock { get; set; } = new();
        public List<StockValidationDetail> ValidationDetails { get; set; } = new();
    }

    /// <summary>
    /// Detailed validation result for a single product
    /// </summary>
    public class StockValidationDetail
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsAvailable { get; set; }
    }

    /// <summary>
    /// Product stock information
    /// </summary>
    public class ProductStockInfo
    {
        public Guid ProductId { get; set; }
        public int AvailableQuantity { get; set; }
        public int TotalReserved { get; set; }
        public int ActiveReservations { get; set; }
    }

    /// <summary>
    /// Simple stock check result
    /// </summary>
    public class StockCheckResult
    {
        public Guid ProductId { get; set; }
        public int RequestedQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableQuantity { get; set; }
    }

    /// <summary>
    /// Stock reservation information
    /// </summary>
    public class StockReservationInfo
    {
        public Guid Id { get; set; }
        public Guid CheckoutId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Cleanup operation result
    /// </summary>
    public class CleanupResult
    {
        public int ReservationsCleaned { get; set; }
        public DateTime CleanedAt { get; set; }
    }

    #endregion
}