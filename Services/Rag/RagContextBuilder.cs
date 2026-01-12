using ALOud.Services;

namespace ALOud.Services.Rag;

public sealed class RagContextBuilder
{
    private readonly CartService _cartService;
    private readonly IProductService _productService;

    public RagContextBuilder(
        CartService cartService,
        IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
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
