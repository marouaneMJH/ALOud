using ALOud.Services.Infrastructure.Rag.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ALOud.Services.Rag;

public static class RagToolCatalog
{
    // Full catalog for general queries
    public static readonly IReadOnlyList<RagToolDefinition> All = [
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
        CompareProducts(),
    ];


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
