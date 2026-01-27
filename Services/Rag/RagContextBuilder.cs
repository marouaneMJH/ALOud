namespace ALOud.Services.Rag;

public sealed class RagContextBuilder
{
    private readonly CartService _cartService;

    public RagContextBuilder(CartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<object> BuildAsync()
    {
        var cart = await _cartService.GetCartAsync();

        // Include essential fields for display
        return new
        {
            items = cart.Select(i => new
            {
                id = i.ProductId,
                productName = i.ProductName,
                qty = i.Quantity,
                price = i.Price,
                total = i.Total
            }).ToList(),
            count = cart.Sum(i => i.Quantity),
            totalPrice = cart.Sum(i => i.Total)
        };
    }
}
