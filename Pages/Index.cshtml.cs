using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ALOud.Data;
using Services;
using ViewModels;

namespace Pages
{
    public class IndexModel : PageModel
    {
        private readonly ALOudDbContext _context;
        private readonly CartService _cartService;
        private readonly ICacheService _cache;

        public IndexModel(ALOudDbContext context, CartService cartService, ICacheService cache)
        {
            _context = context;
            _cartService = cartService;
            _cache = cache;
        }

        public List<HomeProductVM> Products { get; set; } = new();

        public SearchVM Search { get; set; } = new();

        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 12;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public async Task OnGetAsync(string? query, decimal? minPrice, decimal? maxPrice, string? sort, int pageIndex = 1, int pageSize = 12)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;

            Search = new SearchVM
            {
                Query = query,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort
            };

            // Build a cache key based on the query parameters
            string key = $"products:list:q={query ?? ""}:min={minPrice?.ToString() ?? ""}:max={maxPrice?.ToString() ?? ""}:s={sort ?? ""}:p={pageIndex}:ps={pageSize}";

            var cached = await _cache.GetAsync<(List<HomeProductVM>, int)>(key);
            if (cached.Item1 != null)
            {
                Products = cached.Item1;
                TotalCount = cached.Item2;
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
                return;
            }

            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                productsQuery = productsQuery.Where(p => p.Name.Contains(query));

            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice);

            productsQuery = sort switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                _ => productsQuery.OrderByDescending(p => p.Id)
            };

            TotalCount = await productsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Products = await productsQuery
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new HomeProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                }).ToListAsync();

            // Cache for 5 minutes
            await _cache.SetAsync(key, (Products, TotalCount), TimeSpan.FromMinutes(5));
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

            return RedirectToPage();
        }
    }
}
