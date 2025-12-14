using System.Text.Json;
using VeloStore.ViewModels;

namespace VeloStore.Services
{
    public class CartService
    {
        private readonly ICacheService _cache;
        private readonly IHttpContextAccessor _accessor;
        private const string CartKeyPrefix = "cart:";

        public CartService(ICacheService cache, IHttpContextAccessor accessor)
        {
            _cache = cache;
            _accessor = accessor;
        }

        private string GetCartKey()
        {
            // Try to get user session ID, or create a temporary cart ID
            var sessionId = _accessor.HttpContext?.Session?.Id
                ?? _accessor.HttpContext?.TraceIdentifier
                ?? Guid.NewGuid().ToString();
            
            return CartKeyPrefix + sessionId;
        }

        public async Task<List<CartItemVM>> GetCartAsync()
        {
            var cartKey = GetCartKey();
            var cart = await _cache.GetAsync<List<CartItemVM>>(cartKey);
            return cart ?? new List<CartItemVM>();
        }

        public List<CartItemVM> GetCart()
        {
            return GetCartAsync().Result;
        }

        public async Task SaveCartAsync(List<CartItemVM> cart)
        {
            var cartKey = GetCartKey();

            Console.WriteLine(cartKey);
            Console.WriteLine(cart);

            // Set expiry to 7 days
            await _cache.SetAsync(cartKey, cart, TimeSpan.FromDays(7));
        }

        public void SaveCart(List<CartItemVM> cart)
        {
            SaveCartAsync(cart).Wait();
        }

        public async Task AddToCartAsync(CartItemVM item)
        {
            var cart = await GetCartAsync();
            var existing = cart.FirstOrDefault(p => p.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(item);

            await SaveCartAsync(cart);
        }

        public void AddToCart(CartItemVM item)
        {
            AddToCartAsync(item).Wait();
        }

        public async Task RemoveAsync(int productId)
        {
            var cart = await GetCartAsync();
            cart.RemoveAll(p => p.ProductId == productId);
            await SaveCartAsync(cart);
        }

        public void Remove(int productId)
        {
            RemoveAsync(productId).Wait();
        }

        public async Task IncreaseAsync(int productId)
        {
            var cart = await GetCartAsync();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item != null) item.Quantity++;
            await SaveCartAsync(cart);
        }

        public void Increase(int productId)
        {
            IncreaseAsync(productId).Wait();
        }

        public async Task DecreaseAsync(int productId)
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

        public void Decrease(int productId)
        {
            DecreaseAsync(productId).Wait();
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var cart = await GetCartAsync();
            return cart.Sum(item => item.Quantity);
        }

        public int GetCartItemCount()
        {
            return GetCartItemCountAsync().Result;
        }
    }
}
