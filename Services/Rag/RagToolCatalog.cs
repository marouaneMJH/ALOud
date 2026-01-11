using ALOud.Services.Rag.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ALOud.Services.Rag;

public static class RagToolCatalog
{
    public static readonly IReadOnlyList<RagToolDefinition> All = [
        SearchProducts(),
        GetProductDetails(),
        RecommendProducts(),
        GetCart(),
        AddToCart(),
        RemoveFromCart(),
        Increase(),
        Decrease(),

    ];


    private static RagToolDefinition SearchProducts() => new()
    {
        Name = "search_products",
        Description = "Find products by name/brand. Returns Id, Name, Description, Price, ImageUrl. Always display ImageUrl.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string" }
            },
            required = new[] { "query" }
        }
    };

    private static RagToolDefinition GetProductDetails() => new()
    {
        Name = "get_product_details",
        Description = "Get full product info: Name, Description, Price, Stock, ImageUrl. Always show image.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" }
            },
            required = new[] { "productId" }
        }
    };

    private static RagToolDefinition RecommendProducts() => new()
    {
        Name = "recommend_products",
        Description = "Recommend products by preferences (fresh, woody, etc). Returns products with ImageUrl. Show images.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                preferences = new { type = "string" },
                limit = new { type = "integer", minimum = 1, maximum = 10 }
            },
            required = new[] { "preferences" }
        }
    };

    private static RagToolDefinition GetCart() => new()
    {
        Name = "get_cart",
        Description = "Get cart contents",
        ParametersSchema = new
        {
            type = "object",
            properties = new { },
            required = Array.Empty<string>()
        }
    };

    private static RagToolDefinition AddToCart() => new()
    {
        Name = "add_to_cart",
        Description = "Add product to cart",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" },
                quantity = new { type = "integer", minimum = 1 }
            },
            required = new[] { "productId", "quantity" }
        }
    };

    private static RagToolDefinition RemoveFromCart() => new()
    {
        Name = "remove_from_cart",
        Description = "Remove product from cart",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" }
            },
            required = new[] { "productId" }
        }
    };

    private static RagToolDefinition Increase() => new()
    {
        Name = "increase",
        Description = "Increase quantity by 1",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" }
            },
            required = new[] { "productId" }
        }
    };

    private static RagToolDefinition Decrease() => new()
    {
        Name = "decrease",
        Description = "Decrease quantity by 1",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" }
            },
            required = new[] { "productId" }
        }
    };

}
