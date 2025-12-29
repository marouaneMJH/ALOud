using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using Services;
using ViewModels;

namespace Pages.Product
{
    public class DetailsModel : PageModel
    {
        private readonly ALOudDbContext _context;
        private readonly CartService _cartService;
        private readonly ICacheService _cache;

        public ProductDetailsVM Product { get; set; }

        public DetailsModel(ALOudDbContext context, CartService cartService, ICacheService cache)
        {
            _context = context;
            _cartService = cartService;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string key = $"product:details:{id}";

            var cached = await _cache.GetAsync<ProductDetailsVM>(key);
            if (cached != null)
            {
                Product = cached;
                return Page();
            }

            var p = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return RedirectToPage("/Index");

            Product = new ProductDetailsVM
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            };

            // cache product details for 60 minutes
            await _cache.SetAsync(key, Product, TimeSpan.FromMinutes(60));

            return Page();
        }

        public IActionResult OnPostAddToCart(int productId)
        {
            var product = _context.Products.First(p => p.Id == productId);

            _cartService.AddToCart(new CartItemVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = 1
            });

            return RedirectToPage("/Cart/Index");
        }
    }
}
