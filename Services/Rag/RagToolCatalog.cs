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
        Description = "Search for products by name, brand, or description. Returns product ID, name, price, stock, and category. ALWAYS use this before adding products to cart.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string", description = "Search query (product name, brand, or keyword)" }
            },
            required = new[] { "query" }
        }
    };

    private static RagToolDefinition GetProductDetails() => new()
    {
        Name = "get_product_details",
        Description = "Get complete details about a specific product including description, price, stock, image URL, and category. Use this when user asks about a product or wants more information.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                productId = new { type = "integer", description = "Product ID to get details for" }
            },
            required = new[] { "productId" }
        }
    };

    private static RagToolDefinition RecommendProducts() => new()
    {
        Name = "recommend_products",
        Description = "Get personalized product recommendations based on user preferences or cart contents. Use when user asks for suggestions, recommendations, or describes what they're looking for (e.g., 'something fresh', 'for summer', 'woody scents').",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                preferences = new { type = "string", description = "User preferences or description of what they're looking for" },
                limit = new { type = "integer", description = "Maximum number of recommendations (default 5)", minimum = 1, maximum = 10 }
            },
            required = new[] { "preferences" }
        }
    };

    private static RagToolDefinition GetCart() => new()
    {
        Name = "get_cart",
        Description = "Get the current shopping cart content",
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
        Description = "Add a product to the cart with a given quantity",
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
        Description = "Remove a product entirely from the cart",
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
        Description = "Increase quantity of a product in the cart by one",
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
        Description = "Decrease quantity of a product in the cart by one; remove if quantity reaches zero",
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
