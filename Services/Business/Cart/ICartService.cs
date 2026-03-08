using ViewModels;

namespace ALOud.Services
{
    public interface ICartService
    {
        Task<List<CartItemVM>> GetCartAsync();
        Task SaveCartAsync(List<CartItemVM> cart);
        Task AddToCartAsync(CartItemVM item);
        Task RemoveAsync(Guid productId);
        Task IncreaseAsync(Guid productId);
        Task DecreaseAsync(Guid productId);
        Task<int> GetCartItemCountAsync();
    }
}
