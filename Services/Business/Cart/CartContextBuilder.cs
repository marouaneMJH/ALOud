using ALOud.Models.Cart;

namespace ALOud.Services.Cart;

public sealed class CartContextBuilder : ICartContextBuilder
{
    private readonly ICartService _cartService;

    public CartContextBuilder(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<CartContext> BuildAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        // TODO: Get cart by user Id
        var cart = await _cartService.GetCartAsync();


        var items = cart.Select(i => new CartItemContext
        {
            // TODO: Add perfume image
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.Price,
            Total = i.Total
        }).ToList();

        return new CartContext
        {
            Items = items,
            TotalQuantity = items.Sum(i => i.Quantity),
            TotalPrice = items.Sum(i => i.Total)
        };
    }
}
