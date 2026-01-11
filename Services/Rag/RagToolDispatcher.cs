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
            "search_products" => await HandleSearchAsync(args),

            "get_product_details" => await HandleGetProductDetailsAsync(args),

            "recommend_products" => await HandleRecommendProductsAsync(args),

            "get_cart" => await _cartService.GetCartAsync(),

            "add_to_cart" => await HandleAddAsync(args),

            "remove_from_cart" => await HandleRemoveAsync(args),

            "increase" => await HandleIncreaseAsync(args),

            "decrease" => await HandleDecreaseAsync(args),

            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }

    private async Task<object> HandleSearchAsync(Dictionary<string, object> args)
    {
        var query = args["query"].ToString() ?? string.Empty;
        var products = await _productService.SearchProductsAsync(query);

        return new
        {
            Products = products.Take(10).ToList()
        };
    }

    private async Task<object> HandleGetProductDetailsAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return new { Error = "Product not found" };

        return new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.ImageUrl,
            InStock = product.InStock
        };
    }

    private async Task<object> HandleRecommendProductsAsync(Dictionary<string, object> args)
    {
        var preferences = args["preferences"].ToString() ?? string.Empty;
        var limit = args.ContainsKey("limit") ? GetInt32(args["limit"]) : 5;

        // Get recommendations based on preferences
        var products = await _productService.SearchProductsAsync(preferences);

        // If no results, get random popular products
        if (!products.Any())
        {
            var allProducts = await _productService.GetAllProductsAsync();
            products = allProducts
                .OrderBy(_ => Guid.NewGuid())
                .Take(limit)
                .Cast<object>()
                .ToList();
        }

        return new
        {
            Recommendations = products.Take(limit).ToList(),
            BasedOn = preferences
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
