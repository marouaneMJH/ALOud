using ALOud.Controllers.Api;
using ALOud.DTOs;
using ALOud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ALOud.Controllers.Api.v1
{
    /// <summary>
    /// API Controller for checkout process management (v1)
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CheckoutController : BaseApiController
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ICartService _cartService;
        private readonly ILogger<CheckoutController> _logger;

        /// <summary>
        /// Initializes a new instance of the CheckoutController class
        /// </summary>
        /// <param name="checkoutService">The checkout service</param>
        /// <param name="cartService">The cart service</param>
        /// <param name="logger">The logger</param>
        public CheckoutController(
            ICheckoutService checkoutService,
            ICartService cartService,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Starts a new checkout process
        /// </summary>
        /// <param name="dto">The checkout initialization data</param>
        /// <returns>The checkout summary</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartCheckout([FromBody] StartCheckoutDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                // Get cart ID from session/cookie (this would depend on your cart implementation)
                var cartId = GetCartId();
                if (string.IsNullOrEmpty(cartId))
                {
                    return ErrorResponse("No cart found. Please add items to cart before checkout.");
                }

                var checkout = await _checkoutService.StartCheckoutAsync(dto, cartId);
                return CreatedAtAction(
                    nameof(GetCheckout),
                    new { id = checkout.Id },
                    new { success = true, data = checkout }
                );
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting checkout for email: {Email}", dto.Email);
                return ErrorResponse("Failed to start checkout", 500);
            }
        }

        /// <summary>
        /// Gets checkout details by ID
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <returns>The checkout details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCheckout(Guid id)
        {
            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null)
                {
                    return NotFoundResponse("Checkout not found");
                }

                // Validate access (user can only access their own checkout or cart-based access for guests)
                if (!await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                return SuccessResponse(checkout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to retrieve checkout", 500);
            }
        }

        /// <summary>
        /// Gets active checkout for current cart
        /// </summary>
        /// <returns>The active checkout if exists</returns>
        [HttpGet("current")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentCheckout()
        {
            try
            {
                var cartId = GetCartId();
                if (string.IsNullOrEmpty(cartId))
                {
                    return NotFoundResponse("No active cart found");
                }

                var checkout = await _checkoutService.GetActiveCheckoutByCartIdAsync(cartId);
                if (checkout == null)
                {
                    return NotFoundResponse("No active checkout found");
                }

                return SuccessResponse(checkout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current checkout");
                return ErrorResponse("Failed to retrieve current checkout", 500);
            }
        }

        /// <summary>
        /// Updates checkout step completion status
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="dto">The step update data</param>
        /// <returns>Updated checkout summary</returns>
        [HttpPut("{id:guid}/steps")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCheckoutSteps(Guid id, [FromBody] UpdateCheckoutStepDto dto)
        {
            if (id != dto.CheckoutId)
            {
                return ErrorResponse("Checkout ID in URL does not match request body");
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var updatedCheckout = await _checkoutService.UpdateCheckoutStepAsync(dto);
                return SuccessResponse(updatedCheckout);
            }
            catch (ArgumentException)
            {
                return NotFoundResponse("Checkout not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checkout steps: {CheckoutId}", id);
                return ErrorResponse("Failed to update checkout steps", 500);
            }
        }

        /// <summary>
        /// Sets shipping address for checkout
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="dto">The shipping address data</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/shipping-address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetShippingAddress(Guid id, [FromBody] CheckoutAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                dto.CheckoutId = id;
                var address = await _checkoutService.SetShippingAddressAsync(id, dto);
                return SuccessResponse("Shipping address set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shipping address for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to set shipping address", 500);
            }
        }

        /// <summary>
        /// Sets billing address for checkout
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="dto">The billing address data</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/billing-address")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetBillingAddress(Guid id, [FromBody] CheckoutAddressDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                dto.CheckoutId = id;
                var address = await _checkoutService.SetBillingAddressAsync(id, dto);
                return SuccessResponse("Billing address set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting billing address for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to set billing address", 500);
            }
        }

        /// <summary>
        /// Copies shipping address to billing address
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/copy-shipping-to-billing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CopyShippingToBilling(Guid id)
        {
            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var success = await _checkoutService.CopyShippingToBillingAsync(id);
                if (!success)
                {
                    return ErrorResponse("Failed to copy shipping address. Make sure shipping address is set first.");
                }

                return SuccessResponse("Billing address copied from shipping address successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying shipping to billing for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to copy addresses", 500);
            }
        }

        /// <summary>
        /// Sets shipping method for checkout
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="shippingMethod">The shipping method name</param>
        /// <returns>Updated checkout summary</returns>
        [HttpPost("{id:guid}/shipping-method")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetShippingMethod(Guid id, [FromBody] string shippingMethod)
        {
            if (string.IsNullOrWhiteSpace(shippingMethod))
            {
                return ErrorResponse("Shipping method is required");
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var updatedCheckout = await _checkoutService.SetShippingMethodAsync(id, shippingMethod);
                return SuccessResponse(updatedCheckout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shipping method for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to set shipping method", 500);
            }
        }

        /// <summary>
        /// Sets payment method for checkout
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="request">The payment method data</param>
        /// <returns>Updated checkout summary</returns>
        [HttpPost("{id:guid}/payment-method")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPaymentMethod(Guid id, [FromBody] SetPaymentMethodRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return ErrorResponse("Payment method is required");
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var updatedCheckout = await _checkoutService.SetPaymentMethodAsync(id, request.PaymentMethod, request.PaymentIntentId);
                return SuccessResponse(updatedCheckout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting payment method for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to set payment method", 500);
            }
        }

        /// <summary>
        /// Gets available shipping methods for checkout
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <returns>List of available shipping methods</returns>
        [HttpGet("{id:guid}/shipping-methods")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetShippingMethods(Guid id)
        {
            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var methods = await _checkoutService.GetAvailableShippingMethodsAsync(id);
                return SuccessResponse(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipping methods for checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to get shipping methods", 500);
            }
        }

        /// <summary>
        /// Gets available payment methods
        /// </summary>
        /// <returns>List of available payment methods</returns>
        [HttpGet("payment-methods")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var methods = await _checkoutService.GetAvailablePaymentMethodsAsync();
                return SuccessResponse(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods");
                return ErrorResponse("Failed to get payment methods", 500);
            }
        }

        /// <summary>
        /// Completes the checkout process
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <param name="dto">The completion data</param>
        /// <returns>Completed checkout summary</returns>
        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(CheckoutSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteCheckout(Guid id, [FromBody] CompleteCheckoutDto dto)
        {
            if (id != dto.CheckoutId)
            {
                return ErrorResponse("Checkout ID in URL does not match request body");
            }

            if (!ModelState.IsValid)
            {
                return ValidationErrorResponse(ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                ));
            }

            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var completedCheckout = await _checkoutService.CompleteCheckoutAsync(dto);
                return SuccessResponse(completedCheckout);
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to complete checkout", 500);
            }
        }

        /// <summary>
        /// Abandons the checkout process
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/abandon")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AbandonCheckout(Guid id)
        {
            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var success = await _checkoutService.AbandonCheckoutAsync(id);
                if (!success)
                {
                    return NotFoundResponse("Checkout not found");
                }

                return SuccessResponse("Checkout abandoned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning checkout: {CheckoutId}", id);
                return ErrorResponse("Failed to abandon checkout", 500);
            }
        }

        /// <summary>
        /// Extends checkout session time
        /// </summary>
        /// <param name="id">The checkout ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("{id:guid}/extend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExtendCheckoutSession(Guid id)
        {
            try
            {
                var checkout = await _checkoutService.GetCheckoutAsync(id);
                if (checkout == null || !await ValidateCheckoutAccess(checkout))
                {
                    return NotFoundResponse("Checkout not found");
                }

                var success = await _checkoutService.ExtendCheckoutSessionAsync(id);
                if (!success)
                {
                    return NotFoundResponse("Checkout not found");
                }

                return SuccessResponse("Checkout session extended successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending checkout session: {CheckoutId}", id);
                return ErrorResponse("Failed to extend checkout session", 500);
            }
        }

        /// <summary>
        /// Validates if the current user/session has access to the checkout
        /// </summary>
        private async Task<bool> ValidateCheckoutAccess(CheckoutSummaryDto checkout)
        {
            // If user is authenticated, check if they own the checkout
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    // For authenticated users, they must own the checkout
                    return checkout.IsGuestCheckout == false; // Additional validation could check actual user ID
                }
            }

            // For guest checkouts, validate by cart ID (session-based)
            var cartId = GetCartId();
            return !string.IsNullOrEmpty(cartId) && await _checkoutService.ValidateCartIntegrityAsync(checkout.Id, cartId);
        }

        /// <summary>
        /// Gets the cart ID from the current session/cookie
        /// This would integrate with your existing cart implementation
        /// </summary>
        private string GetCartId()
        {
            // This should integrate with your existing cart service's cart ID logic
            // For now, return a placeholder - implement based on your cart system
            return HttpContext.Session.GetString("CartId") ?? 
                   HttpContext.Request.Cookies["CartId"] ?? 
                   string.Empty;
        }

        /// <summary>
        /// Request model for setting payment method
        /// </summary>
        public class SetPaymentMethodRequest
        {
            public string PaymentMethod { get; set; } = string.Empty;
            public string? PaymentIntentId { get; set; }
        }
    }
}