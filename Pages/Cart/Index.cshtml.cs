using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ALOud.Services;
using ViewModels;

namespace Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;

        public List<CartItemVM> Items { get; set; } = new();

        public IndexModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public void OnGet()
        {
            Items = _cartService.GetCart();
        }

        public IActionResult OnPostIncrease(Guid productId)
        {
            _cartService.Increase(productId);
            return RedirectToPage();
        }

        public IActionResult OnPostDecrease(Guid productId)
        {
            _cartService.Decrease(productId);
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(Guid productId)
        {
            _cartService.Remove(productId);
            return RedirectToPage();
        }
    }
}
