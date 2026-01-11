using Microsoft.AspNetCore.Mvc;
using ALOud.Services;

namespace ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartCountViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var count = _cartService.GetCartItemCount();
            return View(count);
        }
    }
}