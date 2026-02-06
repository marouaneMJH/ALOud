using ALOud.Models.Cart;

namespace ALOud.Services.Cart;

public interface ICartContextBuilder
{
    Task<CartContext> BuildAsync(Guid userId, CancellationToken ct = default);
}
