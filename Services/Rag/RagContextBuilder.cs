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
        
        // Minimal context - only send IDs and quantities
        return new
        {
            items = cart.Select(i => new { id = i.ProductId, qty = i.Quantity }).ToList(),
            count = cart.Sum(i => i.Quantity)
        };
    }
}
