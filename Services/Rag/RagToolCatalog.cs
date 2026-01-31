using ALOud.Services.Rag.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ALOud.Services.Rag;

public static class RagToolCatalog
{
    // Full catalog for general queries
    public static readonly IReadOnlyList<RagToolDefinition> All = [
        SearchProducts(),
        GetProductDetails(),
        RecommendProducts(),
        GetCart(),
        AddToCart(),
        RemoveFromCart(),
        Increase(),
        Decrease(),
        AnalyzeCart(),
        CompareProducts(),
    ];

    // Optimized subsets for intent-specific routing (reduces token usage)
    public static readonly IReadOnlyList<RagToolDefinition> CartTools = [
        GetCart(),
        AddToCart(),
        RemoveFromCart(),
        Increase(),
        Decrease(),
        AnalyzeCart(),
    ];

    public static readonly IReadOnlyList<RagToolDefinition> SearchTools = [
        SearchProducts(),
        GetProductDetails(),
        RecommendProducts(),
        CompareProducts(),
    ];


    private static RagToolDefinition SearchProducts() => new()
    {
        Name = "search_products",
        Description = "Search products by name/brand. Returns max 5 results with Id, Name, Price, ImageUrl.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string", description = "Search keywords" }
            },
            required = new[] { "query" }
        }
    };

    private static RagToolDefinition GetProductDetails() => new()
    {
        Name = "get_product_details",
        Description = "Complete product details by ID.",
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
        Description = "Recommend perfumes based on preferences (woody, fresh, oriental, floral, spicy).",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                preferences = new { type = "string", description = "Desired perfume type" },
                limit = new { type = "integer", minimum = 1, maximum = 5, @default = 3 }
            },
            required = new[] { "preferences" }
        }
    };

    private static RagToolDefinition GetCart() => new()
    {
        Name = "get_cart",
        Description = "View current cart contents.",
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
        Description = "Add product to cart. Automatically checks stock.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer" },
                quantity = new { type = "integer", minimum = 1, @default = 1 }
            },
            required = new[] { "productId" }
        }
    };

    private static RagToolDefinition RemoveFromCart() => new()
    {
        Name = "remove_from_cart",
        Description = "Remove product from cart.",
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
        Description = "Increase quantity by 1.",
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
        Description = "Decrease quantity by 1.",
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

    // NEW: Smart cart analysis tool
    private static RagToolDefinition AnalyzeCart() => new()
    {
        Name = "analyze_cart",
        Description = "Smart cart analysis: total, item count, stock alerts, suggestions.",
        ParametersSchema = new
        {
            type = "object",
            properties = new { },
            required = Array.Empty<string>()
        }
    };

    // NEW: Price comparison tool
    private static RagToolDefinition CompareProducts() => new()
    {
        Name = "compare_products",
        Description = "Compare 2-3 products by price and features.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productIds = new
                {
                    type = "array",
                    items = new { type = "integer" },
                    minItems = 2,
                    maxItems = 3
                }
            },
            required = new[] { "productIds" }
        }
    };
}
