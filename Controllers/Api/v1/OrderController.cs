using ALOud.Controllers.Api;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for order management (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        /// <summary>
        /// Initializes a new instance of the OrderController class
        /// </summary>
        /// <param name="orderService">The order service</param>
        /// <param name="logger">The logger</param>
        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found");
                }

                // Verify ownership (users can only access their own orders, admins can access all)
                var userId = GetUserId();
                if (!IsAdmin() && order.UserId != userId)
                {
                    return Forbid("You can only access your own orders");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        /// <param name="orderNumber">The order number</param>
        /// <returns>Order details</returns>
        [HttpGet("by-number/{orderNumber}")]
        [Authorize]
        public async Task<IActionResult> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _orderService.GetOrderByNumberAsync(orderNumber);
                if (order == null)
                {
                    return NotFound($"Order with number {orderNumber} not found");
                }

                // Verify ownership
                var userId = GetUserId();
                if (!IsAdmin() && order.UserId != userId)
                {
                    return Forbid("You can only access your own orders");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user's orders with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of user orders</returns>
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders(int page = 1, int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var userId = GetUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", GetUserId());
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search orders (admin only)
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Search results</returns>
        [HttpPost("search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchOrders([FromBody] OrderSearchDto searchDto)
        {
            try
            {
                var orders = await _orderService.SearchOrdersAsync(searchDto);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders with criteria: {@SearchCriteria}", searchDto);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update order status (admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="statusDto">Status update information</param>
        /// <returns>Updated order</returns>
        [HttpPut("{orderId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                statusDto.OrderId = orderId; // Set the order ID from the route parameter
                var success = await _orderService.UpdateOrderStatusAsync(statusDto, GetUserId());
                if (!success)
                {
                    return BadRequest("Unable to update order status - check order exists and status transition is valid");
                }

                // Return the updated order
                var updatedOrder = await _orderService.GetOrderAsync(orderId);
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="cancelDto">Cancellation information</param>
        /// <returns>Updated order</returns>
        [HttpPost("{orderId:guid}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderDto cancelDto)
        {
            try
            {
                // Get order to verify ownership
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found");
                }

                // Verify ownership (users can only cancel their own orders, admins can cancel any)
                var userId = GetUserId();
                if (!IsAdmin() && order.UserId != userId)
                {
                    return Forbid("You can only cancel your own orders");
                }

                var success = await _orderService.CancelOrderAsync(cancelDto, userId);
                if (!success)
                {
                    return BadRequest("Unable to cancel order - check order exists and is cancellable");
                }

                // Return the updated order
                var updatedOrder = await _orderService.GetOrderAsync(orderId);
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order tracking information
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order tracking details</returns>
        [HttpGet("{orderId:guid}/tracking")]
        [Authorize]
        public async Task<IActionResult> GetOrderTracking(Guid orderId)
        {
            try
            {
                // Get order to verify ownership
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found");
                }

                // Verify ownership
                var userId = GetUserId();
                if (!IsAdmin() && order.UserId != userId)
                {
                    return Forbid("You can only access tracking for your own orders");
                }

                var tracking = await _orderService.GetOrderTrackingAsync(orderId);
                return Ok(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tracking for order {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order statistics (admin only)
        /// </summary>
        /// <param name="days">Number of days to analyze (default: 30)</param>
        /// <returns>Order statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStatistics(int days = 30)
        {
            try
            {
                if (days < 1 || days > 365) days = 30;

                var statistics = await _orderService.GetOrderStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics for {Days} days", days);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Mark order as shipped (admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="shipmentDto">Shipping information</param>
        /// <returns>Success status</returns>
        [HttpPost("{orderId:guid}/ship")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ShipOrder(Guid orderId, [FromBody] ShipOrderDto shipmentDto)
        {
            try
            {
                var success = await _orderService.MarkOrderAsShippedAsync(orderId, shipmentDto.TrackingNumber, shipmentDto.Carrier, shipmentDto.ShippingMethod);
                if (!success)
                {
                    return BadRequest("Unable to mark order as shipped - check order status");
                }

                return Ok(new { message = "Order marked as shipped successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as shipped for {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Mark order as delivered (admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{orderId:guid}/deliver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsDelivered(Guid orderId)
        {
            try
            {
                var success = await _orderService.MarkOrderAsDeliveredAsync(orderId);
                if (!success)
                {
                    return BadRequest("Unable to mark order as delivered - check order status");
                }

                return Ok(new { message = "Order marked as delivered successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as delivered for {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <returns>User ID</returns>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user token");
        }

        /// <summary>
        /// Check if current user is admin
        /// </summary>
        /// <returns>True if user is admin</returns>
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion
    }
}