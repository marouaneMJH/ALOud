using System.Security.Claims;
using ALOud.DTOs;
using ALOud.Services;
using ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for user order management
    /// </summary>
    [Authorize]
    public class OrderController : Controller
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
        /// Displays the user's order history
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>Order history view</returns>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            try
            {
                // Guard against invalid page numbers that would crash EF Core .Skip() calculation
                page = Math.Max(1, page);

                var userId = GetUserId();
                var orders = await _orderService.GetFullUserOrdersAsync(userId, page, pageSize);
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.HasPagination = orders.Count == pageSize; // Simplistic pagination check

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order history");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Displays the details of a specific order
        /// </summary>
        /// <param name="id">The order identifier</param>
        /// <returns>Order details view</returns>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orderService.GetOrderAsync(id);

                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("Order not found or access denied: {OrderId}", id);
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details: {OrderId}", id);
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Displays tracking information for an order
        /// </summary>
        /// <param name="orderNumber">The order number</param>
        /// <returns>Order tracking view</returns>
        [HttpGet]
        public async Task<IActionResult> Track(string orderNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNumber))
                {
                    return BadRequest("Order number is required");
                }

                var order = await _orderService.GetOrderByNumberAsync(orderNumber);
                var userId = GetUserId();

                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("Order not found or access denied for tracking: {OrderNumber}", orderNumber);
                    return NotFound();
                }

                // Track.cshtml expects OrderDto. It already contains tracking fields.
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order tracking: {OrderNumber}", orderNumber);
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Processes order cancellation
        /// </summary>
        /// <param name="id">The order identifier</param>
        /// <returns>Redirect to details or error</returns>
        [HttpGet]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orderService.GetOrderAsync(id);

                if (order == null || order.UserId != userId)
                {
                    return NotFound();
                }

                if (!order.IsCancellable)
                {
                    TempData["ErrorMessage"] = "This order cannot be cancelled in its current status.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var cancelDto = new CancelOrderDto
                {
                    OrderId = id,
                    Reason = "Cancelled by user via web interface",
                    NotifyCustomer = true,
                    RefundPayment = true
                };

                var result = await _orderService.CancelOrderAsync(cancelDto, userId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Your order has been cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel the order. Please contact support.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {OrderId}", id);
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        /// <returns>The user ID</returns>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in claims.");
        }
    }
}
