using Services;
using ALOud.Services.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class RagContextBuilder
{
    private readonly CartService _cartService;
    private readonly ProductService _productService;

    public RagContextBuilder(
        CartService cartService,
        ProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<object> BuildAsync()
    {
        var cart = await _cartService.GetCartAsync();

        var products = new List<object>();

        foreach (var item in cart)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null) continue;

            products.Add(new
            {
                product.Id,
                product.Name,
                product.Price,
                item.Quantity,
                Total = product.Price * item.Quantity
            });
        }

        return new
        {
            Cart = products,
            ItemCount = cart.Sum(i => i.Quantity),
            GrandTotal = products.Sum(p => (decimal)p.GetType().GetProperty("Total")!.GetValue(p)!)
        };
    }
}
