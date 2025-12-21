using Microsoft.AspNetCore.Mvc;
using VeloStore.Services;

namespace VeloStore.ViewComponents
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