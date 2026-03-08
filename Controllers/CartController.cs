using Microsoft.AspNetCore.Mvc;
using ALOud.Services;
using ViewModels;

namespace ALOud.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var items = await _cartService.GetCartAsync();
            return View(items);
        }

        // POST: /Cart/Increase
        [HttpPost]
        public async Task<IActionResult> Increase(Guid productId)
        {
            await _cartService.IncreaseAsync(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Decrease
        [HttpPost]
        public async Task<IActionResult> Decrease(Guid productId)
        {
            await _cartService.DecreaseAsync(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(Guid productId)
        {
            await _cartService.RemoveAsync(productId);
            return RedirectToAction(nameof(Index));
        }
    }
}
