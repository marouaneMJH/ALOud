using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services;
using ALOud.DTOs;
using System.Security.Claims;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for the full checkout workflow
    /// </summary>
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICheckoutService _checkoutService;
        private readonly IUserAddressService _userAddressService;
        private readonly ILogger<CheckoutController> _logger;

        /// <summary>
        /// Initializes a new instance of the CheckoutController class
        /// </summary>
        public CheckoutController(
            ICartService cartService,
            ICheckoutService checkoutService,
            IUserAddressService userAddressService,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _checkoutService = checkoutService;
            _userAddressService = userAddressService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the checkout page with cart summary and address/payment forms
        /// </summary>
        [HttpGet]
        [Route("Checkout", Name = "MvcCheckoutIndex")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cartItems = await _cartService.GetCartAsync();
                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty. Add some items before checkout.";
                    return RedirectToRoute("MvcCartIndex");
                }

                var subtotal = cartItems.Sum(i => i.Total);
                ViewBag.CartItems = cartItems;
                ViewBag.Subtotal = subtotal;
                ViewBag.Tax = subtotal * 0.08m;
                ViewBag.Shipping = 0m;
                ViewBag.Total = subtotal + ViewBag.Tax;

                // Load user addresses for address selection
                var userId = GetUserId();
                if (userId != Guid.Empty)
                {
                    ViewBag.UserAddresses = await _userAddressService.GetUserAddressesAsync(userId);
                }

                // Load available payment methods
                ViewBag.PaymentMethods = await _checkoutService.GetAvailablePaymentMethodsAsync();

                // Check for active checkout session
                var cartId = GetCartId();
                if (!string.IsNullOrEmpty(cartId))
                {
                    var activeCheckout = await _checkoutService.GetActiveCheckoutByCartIdAsync(cartId);
                    ViewBag.ActiveCheckout = activeCheckout;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["Error"] = "Unable to load checkout. Please try again.";
                return RedirectToRoute("MvcCartIndex");
            }
        }

        /// <summary>
        /// Starts a new checkout session from the current cart
        /// </summary>
        [HttpPost]
        [Route("Checkout/Start", Name = "MvcCheckoutStart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCheckout(StartCheckoutDto dto)
        {
            try
            {
                var cartId = GetCartId();
                if (string.IsNullOrEmpty(cartId))
                {
                    TempData["Error"] = "No cart found. Please add items before checkout.";
                    return RedirectToRoute("MvcCartIndex");
                }

                var checkout = await _checkoutService.StartCheckoutAsync(dto, cartId);
                TempData["Success"] = "Checkout session started.";
                TempData["CheckoutId"] = checkout.Id.ToString();
                return RedirectToRoute("MvcCheckoutIndex");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToRoute("MvcCheckoutIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting checkout");
                TempData["Error"] = "Failed to start checkout. Please try again.";
                return RedirectToRoute("MvcCartIndex");
            }
        }

        /// <summary>
        /// Sets the shipping address on the active checkout
        /// </summary>
        [HttpPost]
        [Route("Checkout/SetShippingAddress", Name = "MvcCheckoutSetShippingAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetShippingAddress(Guid checkoutId, CheckoutAddressDto dto)
        {
            try
            {
                dto.CheckoutId = checkoutId;
                await _checkoutService.SetShippingAddressAsync(checkoutId, dto);
                TempData["Success"] = "Shipping address saved.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shipping address for checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to save shipping address.";
            }

            return RedirectToRoute("MvcCheckoutIndex");
        }

        /// <summary>
        /// Sets the billing address on the active checkout
        /// </summary>
        [HttpPost]
        [Route("Checkout/SetBillingAddress", Name = "MvcCheckoutSetBillingAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBillingAddress(Guid checkoutId, CheckoutAddressDto dto)
        {
            try
            {
                dto.CheckoutId = checkoutId;
                await _checkoutService.SetBillingAddressAsync(checkoutId, dto);
                TempData["Success"] = "Billing address saved.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting billing address for checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to save billing address.";
            }

            return RedirectToRoute("MvcCheckoutIndex");
        }

        /// <summary>
        /// Copies shipping address to billing address
        /// </summary>
        [HttpPost]
        [Route("Checkout/CopyShippingToBilling", Name = "MvcCheckoutCopyAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyShippingToBilling(Guid checkoutId)
        {
            try
            {
                var success = await _checkoutService.CopyShippingToBillingAsync(checkoutId);
                TempData[success ? "Success" : "Error"] = success
                    ? "Billing address copied from shipping."
                    : "Please set a shipping address first.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying address for checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to copy address.";
            }

            return RedirectToRoute("MvcCheckoutIndex");
        }

        /// <summary>
        /// Sets the shipping method on the active checkout
        /// </summary>
        [HttpPost]
        [Route("Checkout/SetShippingMethod", Name = "MvcCheckoutSetShippingMethod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetShippingMethod(Guid checkoutId, string shippingMethod)
        {
            try
            {
                await _checkoutService.SetShippingMethodAsync(checkoutId, shippingMethod);
                TempData["Success"] = "Shipping method updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shipping method for checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to update shipping method.";
            }

            return RedirectToRoute("MvcCheckoutIndex");
        }

        /// <summary>
        /// Sets the payment method on the active checkout
        /// </summary>
        [HttpPost]
        [Route("Checkout/SetPaymentMethod", Name = "MvcCheckoutSetPaymentMethod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPaymentMethod(Guid checkoutId, string paymentMethod, string? paymentIntentId)
        {
            try
            {
                await _checkoutService.SetPaymentMethodAsync(checkoutId, paymentMethod, paymentIntentId);
                TempData["Success"] = "Payment method updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting payment method for checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to update payment method.";
            }

            return RedirectToRoute("MvcCheckoutIndex");
        }

        /// <summary>
        /// Completes the checkout and creates the order
        /// </summary>
        [HttpPost]
        [Route("Checkout/Complete", Name = "MvcCheckoutComplete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteCheckout(CompleteCheckoutDto dto)
        {
            try
            {
                var result = await _checkoutService.CompleteCheckoutAsync(dto);
                return RedirectToRoute("MvcCheckoutSuccess", new { checkoutId = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToRoute("MvcCheckoutIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout {CheckoutId}", dto.CheckoutId);
                TempData["Error"] = "Failed to complete checkout. Please try again.";
                return RedirectToRoute("MvcCheckoutIndex");
            }
        }

        /// <summary>
        /// Abandons the current checkout session
        /// </summary>
        [HttpPost]
        [Route("Checkout/Abandon", Name = "MvcCheckoutAbandon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AbandonCheckout(Guid checkoutId)
        {
            try
            {
                await _checkoutService.AbandonCheckoutAsync(checkoutId);
                TempData["Info"] = "Checkout session cancelled. Your cart items are still saved.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning checkout {CheckoutId}", checkoutId);
                TempData["Error"] = "Failed to cancel checkout.";
            }

            return RedirectToRoute("MvcCartIndex");
        }

        /// <summary>
        /// Handles checkout completion redirect
        /// </summary>
        [HttpGet]
        [Route("Checkout/Success/{checkoutId}", Name = "MvcCheckoutSuccess")]
        public IActionResult Success(Guid checkoutId)
        {
            try
            {
                ViewBag.CheckoutId = checkoutId;
                ViewBag.OrderNumber = $"ALO-{DateTime.Now:yyyyMMdd}-{checkoutId.ToString("N")[..8].ToUpper()}";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout success page for {CheckoutId}", checkoutId);
                TempData["Error"] = "Unable to load order confirmation.";
                return RedirectToRoute("MvcCartIndex");
            }
        }

        /// <summary>
        /// Handles checkout cancellation
        /// </summary>
        [HttpGet]
        [Route("Checkout/Cancel", Name = "MvcCheckoutCancel")]
        public IActionResult Cancel()
        {
            TempData["Info"] = "Checkout was cancelled. Your cart items are still saved.";
            return RedirectToRoute("MvcCartIndex");
        }

        #region Private Helpers

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        private string GetCartId()
        {
            return HttpContext.Request.Cookies["CartId"] ?? string.Empty;
        }

        #endregion
    }
}