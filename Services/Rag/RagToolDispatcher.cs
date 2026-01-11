using ALOud.Services;
using System.Text.Json;

namespace ALOud.Services.Rag;

public sealed class RagToolDispatcher
{
    private readonly CartService _cartService;
    private readonly IProductService _productService;

    public RagToolDispatcher(
        CartService cartService,
        IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<object?> DispatchAsync(string toolName, Dictionary<string, object> args)
    {
        return toolName switch
        {
            "get_cart" => await _cartService.GetCartAsync(),

            "add_to_cart" => await HandleAddAsync(args),

            "remove_from_cart" => await HandleRemoveAsync(args),

            "increase" => await HandleIncreaseAsync(args),

            "decrease" => await HandleDecreaseAsync(args),

            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }

    private async Task<object> HandleAddAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        var quantity = GetInt32(args["quantity"]);

        var product = await _productService.GetProductByIdAsync(productId)
            ?? throw new InvalidOperationException("Product not found");

        for (int i = 0; i < quantity; i++)
        {
            await _cartService.AddToCartAsync(new ViewModels.CartItemVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }

        return new { Success = true };
    }

    private async Task<object> HandleRemoveAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.RemoveAsync(productId);
        return new { Success = true };
    }

    private async Task<object> HandleIncreaseAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.IncreaseAsync(productId);
        return new { Success = true };
    }

    private async Task<object> HandleDecreaseAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.DecreaseAsync(productId);
        return new { Success = true };
    }

    private static int GetInt32(object value)
    {
        if (value is JsonElement element)
            return element.GetInt32();
        return Convert.ToInt32(value);
    }
}
