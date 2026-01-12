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
        Description = "Chercher produits par nom/marque. Retourne max 5 résultats avec Id, Name, Price, ImageUrl.",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string", description = "Mots-clés de recherche" }
            },
            required = new[] { "query" }
        }
    };

    private static RagToolDefinition GetProductDetails() => new()
    {
        Name = "get_product_details",
        Description = "Détails complets d'un produit par son ID.",
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
        Description = "Recommander parfums selon préférences (boisé, frais, oriental, floral, épicé).",
        ParametersSchema = new
        {
            type = "object",
            properties = new
            {
                preferences = new { type = "string", description = "Type de parfum souhaité" },
                limit = new { type = "integer", minimum = 1, maximum = 5, @default = 3 }
            },
            required = new[] { "preferences" }
        }
    };

    private static RagToolDefinition GetCart() => new()
    {
        Name = "get_cart",
        Description = "Voir contenu du panier actuel.",
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
        Description = "Ajouter produit au panier. Vérifie le stock automatiquement.",
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
        Description = "Retirer produit du panier.",
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
        Description = "Augmenter quantité de 1.",
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
        Description = "Diminuer quantité de 1.",
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
        Description = "Analyse intelligente du panier: total, nombre d'articles, alertes stock, suggestions.",
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
        Description = "Comparer 2-3 produits par prix et caractéristiques.",
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
