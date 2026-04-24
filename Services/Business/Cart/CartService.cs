using System.Text.Json;
using ViewModels;
using ALOud.Services.Infrastructure.Cache;

namespace ALOud.Services
{
    // Service: manages shopping cart persisted via cache and cookies.
    public class CartService : ICartService
    {
        private readonly ICacheService _cache;
        private readonly IHttpContextAccessor _accessor;
        private const string CartKeyPrefix = "cart:";

        public CartService(ICacheService cache, IHttpContextAccessor accessor)
        {
            _cache = cache;
            _accessor = accessor;
        }




        public async Task<List<CartItemVM>> GetCartAsync()
        {
            var cartKey = GetCartKey();
            var cart = await _cache.GetAsync<List<CartItemVM>>(cartKey);
            return cart ?? new List<CartItemVM>();
        }
        // Async: retrieve current user's cart from cache (or empty list).

        public async Task SaveCartAsync(List<CartItemVM> cart)
        {
            var cartKey = GetCartKey();
            Console.WriteLine("cartKey" + cartKey);

            // Set expiry to 7 days
            await _cache.SetAsync(cartKey, cart, TimeSpan.FromDays(7));
        }
        // Persist cart to cache with 7-day expiry.

        public async Task AddToCartAsync(CartItemVM item)
        {
            var cart = await GetCartAsync();
            var existing = cart.FirstOrDefault(p => p.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);

            await SaveCartAsync(cart);
        }
        // Add item to cart (async), increment quantity if exists.

        public async Task RemoveAsync(Guid productId)
        {
            var cart = await GetCartAsync();
            cart.RemoveAll(p => p.ProductId == productId);
            await SaveCartAsync(cart);
        }
        // Remove all entries for a product id from the cart (async).

        public async Task IncreaseAsync(Guid productId)
        {
            var cart = await GetCartAsync();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item != null) item.Quantity++;
            await SaveCartAsync(cart);
        }
        // Increase quantity for a product in the cart (async).

        public async Task DecreaseAsync(Guid productId)
        {
            var cart = await GetCartAsync();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    cart.Remove(item);
            }
            await SaveCartAsync(cart);
        }
        // Decrease quantity for a product; remove if quantity <= 0 (async).

        public async Task<int> GetCartItemCountAsync()
        {
            var cart = await GetCartAsync();
            return cart.Sum(item => item.Quantity);
        }
        // Return total number of items in cart (async).

        // Get the Cart key from the cookie
        private string GetCartKey()
        {
            // TODO: merge the unauth user chart with the it auth for the first time
            const string CartIdCookie = "CartId";
            var httpContext = _accessor.HttpContext;

            // For non-HTTP contexts (tests) return a transient key.
            if (httpContext == null)
                return CartKeyPrefix + Guid.NewGuid().ToString();

            // Try to get existing cart ID from cookie
            if (!httpContext.Request.Cookies.TryGetValue(CartIdCookie, out var cartId)
                || string.IsNullOrEmpty(cartId))
            {
                // Generate new cart ID and store in cookie
                cartId = Guid.NewGuid().ToString();
                httpContext.Response.Cookies.Append(CartIdCookie, cartId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }

            return CartKeyPrefix + cartId;
        }
    }
}
