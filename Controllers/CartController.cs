using Microsoft.AspNetCore.Mvc;
using ALOud.Services;
using ViewModels;

namespace ALOud.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(CartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var items = _cartService.GetCart();
            return View(items);
        }

        // POST: /Cart/Increase
        [HttpPost]
        public IActionResult Increase(Guid productId)
        {
            _cartService.Increase(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Decrease
        [HttpPost]
        public IActionResult Decrease(Guid productId)
        {
            _cartService.Decrease(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Remove
        [HttpPost]
        public IActionResult Remove(Guid productId)
        {
            _cartService.Remove(productId);
            return RedirectToAction(nameof(Index));
        }
    }
}
