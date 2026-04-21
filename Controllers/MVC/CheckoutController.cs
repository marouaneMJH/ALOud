using Microsoft.AspNetCore.Mvc;
using ALOud.Services;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for checkout process
    /// </summary>
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICheckoutService _checkoutService;
        private readonly IUserAddressService _userAddressService;
        private readonly ILogger<CheckoutController> _logger;

        /// <summary>
        /// Initializes a new instance of the CheckoutController class
        /// </summary>
        /// <param name="cartService">The cart service</param>
        /// <param name="checkoutService">The checkout service</param>
        /// <param name="userAddressService">The user address service</param>
        /// <param name="logger">The logger</param>
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
        /// Displays the checkout page
        /// </summary>
        /// <returns>Checkout view</returns>
        [HttpGet]
        [Route("Checkout", Name = "MvcCheckoutIndex")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Verify cart has items
                var cartItems = await _cartService.GetCartAsync();
                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty. Add some items before checkout.";
                    return RedirectToRoute("MvcCartIndex");
                }

                // Calculate cart totals for display
                var subtotal = cartItems.Sum(i => i.Total);
                ViewBag.CartItems = cartItems;
                ViewBag.Subtotal = subtotal;
                ViewBag.Tax = subtotal * 0.08m; // 8% flat rate tax
                ViewBag.Shipping = 0m; // Free shipping
                ViewBag.Total = subtotal + ViewBag.Tax;

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
        /// Handles checkout completion redirect
        /// </summary>
        /// <param name="checkoutId">The checkout session ID</param>
        /// <returns>Success or error view</returns>
        [HttpGet]
        [Route("Checkout/Success/{checkoutId}", Name = "MvcCheckoutSuccess")]
        public IActionResult Success(Guid checkoutId)
        {
            try
            {
                // TODO: Implement order confirmation retrieval
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
        /// <returns>Redirect to cart</returns>
        [HttpGet]
        [Route("Checkout/Cancel", Name = "MvcCheckoutCancel")]
        public IActionResult Cancel()
        {
            TempData["Info"] = "Checkout was cancelled. Your cart items are still saved.";
            return RedirectToRoute("MvcCartIndex");
        }
    }
}