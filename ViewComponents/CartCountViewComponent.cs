using Microsoft.AspNetCore.Mvc;
using ALOud.Services;

namespace ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartCountViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var count = await _cartService.GetCartItemCountAsync();
            return Content(count.ToString());
        }
    }
}