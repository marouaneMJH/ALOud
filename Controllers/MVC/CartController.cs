using Microsoft.AspNetCore.Mvc;
using ALOud.Services;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for shopping cart management
    /// </summary>
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        /// <summary>
        /// Initializes a new instance of the CartController class
        /// </summary>
        /// <param name="cartService">The cart service</param>
        /// <param name="logger">The logger</param>
        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the shopping cart
        /// </summary>
        /// <returns>Cart view with items</returns>
        [HttpGet]
        [Route("Cart", Name = "MvcCartIndex")]
        public async Task<IActionResult> Index()
        {
            var items = await _cartService.GetCartAsync();
            return View(items);
        }

        /// <summary>
        /// Increases the quantity of an item in the cart
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Redirect to cart index</returns>
        [HttpPost]
        [Route("Cart/Increase", Name = "MvcCartIncrease")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(Guid productId)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Invalid request");
                return RedirectToRoute("MvcCartIndex");
            }

            if (productId == Guid.Empty)
            {
                SetErrorMessage("Invalid product ID");
                return RedirectToRoute("MvcCartIndex");
            }

            try
            {
                await _cartService.IncreaseAsync(productId);
                SetSuccessMessage("Item quantity increased");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error increasing cart item: {ProductId}", productId);
                SetErrorMessage("Failed to increase item quantity");
            }

            return RedirectToRoute("MvcCartIndex");
        }

        /// <summary>
        /// Decreases the quantity of an item in the cart
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Redirect to cart index</returns>
        [HttpPost]
        [Route("Cart/Decrease", Name = "MvcCartDecrease")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrease(Guid productId)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Invalid request");
                return RedirectToRoute("MvcCartIndex");
            }

            if (productId == Guid.Empty)
            {
                SetErrorMessage("Invalid product ID");
                return RedirectToRoute("MvcCartIndex");
            }

            try
            {
                await _cartService.DecreaseAsync(productId);
                SetSuccessMessage("Item quantity decreased");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decreasing cart item: {ProductId}", productId);
                SetErrorMessage("Failed to decrease item quantity");
            }

            return RedirectToRoute("MvcCartIndex");
        }

        /// <summary>
        /// Removes an item from the cart
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Redirect to cart index</returns>
        [HttpPost]
        [Route("Cart/Remove", Name = "MvcCartRemove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid productId)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Invalid request");
                return RedirectToRoute("MvcCartIndex");
            }

            if (productId == Guid.Empty)
            {
                SetErrorMessage("Invalid product ID");
                return RedirectToRoute("MvcCartIndex");
            }

            try
            {
                await _cartService.RemoveAsync(productId);
                SetSuccessMessage("Item removed from cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item: {ProductId}", productId);
                SetErrorMessage("Failed to remove item from cart");
            }

            return RedirectToRoute("MvcCartIndex");
        }

        #region Private Helper Methods

        /// <summary>
        /// Sets a success message in TempData
        /// </summary>
        /// <param name="message">The success message</param>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message in TempData
        /// </summary>
        /// <param name="message">The error message</param>
        private void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        #endregion
    }
}
